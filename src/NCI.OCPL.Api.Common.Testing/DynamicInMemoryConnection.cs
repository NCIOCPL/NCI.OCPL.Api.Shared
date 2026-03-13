#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Transport;

namespace NCI.OCPL.Api.Common.Testing;


/// <summary>
/// Helper class to create an in-memory connection with a callback to create the response body at runtime.
/// </summary>
/// <remarks>
/// This class is based on the InMemoryConnection class provided by the Elastic library, but allows for dynamic response
/// bodies based on the request body.
/// </remarks>
public class DynamicInMemoryConnection : IRequestInvoker
{
  private static readonly byte[] EmptyBody = Encoding.UTF8.GetBytes("");

  /// The Elasticsearch client will throw an exception if the response doesn't include an
  /// `x-elastic-product` header with a value of `Elasticsearch`.  Rather than litter tests
  /// with a bunch of code to add this header, we'll add it in the connection class.
  private static readonly Dictionary<string, IEnumerable<string>> Headers = new Dictionary<string, IEnumerable<string>>
        {
          { "x-elastic-product", new[] { "Elasticsearch" } }
        };

  /// <summary>
  /// We really just want the response factory from InMemoryRequestInvoker, but the DefaultResponseFactory class is internal
  /// to the Elastic assembly, so we use it directly. Additionally, while InMemoryRequestInvoker does expose Request and
  /// BuildResponse methods, they're not virtual, so we can't inherit.
  ///
  /// Instead, we create an instance of InMemoryRequestInvoker, create our own Request and BuildResponse methods
  /// and use them to invoke response factory.
  /// own request
  /// </summary>
  InMemoryRequestInvoker _innerConnection;

  /// <summary>
  /// The response factory used to do the heavy lifting of building responses.  The class used by _innerConnection is internal to the Elastic library,
  /// so we can't instantiate it directly, but it's exposed by InMemoryRequestInvoker, which allows us to delegate to it.
  /// </summary>
  public ResponseFactory ResponseFactory { get; }

  Func<PostData?, JsonNode, string> _responseBuilder;
  int _returnedStatusCode;
  string _returnedContentType;

  /// <summary>
  /// Initializes a new instance of the <see cref="DynamicInMemoryConnection"/> class.
  /// </summary>
  /// <param name="responseBuilder">A callback function that builds the response body as a string.</param>
  /// <param name="statusCode">The desired HTTP status code.</param>
  /// <param name="contentType">The content type of the mocked response.</param>
  public DynamicInMemoryConnection(Func<PostData?, JsonNode, string> responseBuilder, int statusCode = 200, string contentType = "application/json")
  {
    _innerConnection = new InMemoryRequestInvoker();
    this.ResponseFactory = _innerConnection.ResponseFactory;

    _responseBuilder = responseBuilder;
    _returnedStatusCode = statusCode;
    _returnedContentType = contentType;
  }

  /// <inheritdoc cref="IRequestInvoker.Request{TResponse}"/>
  public virtual TResponse Request<TResponse>(Endpoint endpoint, BoundConfiguration boundConfiguration, PostData? postData)
  where TResponse : TransportResponse, new() =>
    BuildResponse<TResponse>(endpoint, boundConfiguration, postData);


  /// <inheritdoc cref="IRequestInvoker.RequestAsync{TResponse}"/>
  public virtual Task<TResponse> RequestAsync<TResponse>(Endpoint endpoint, BoundConfiguration boundConfiguration, PostData? postData, CancellationToken cancellationToken)
		where TResponse : TransportResponse, new() =>
		BuildResponseAsync<TResponse>(endpoint, boundConfiguration, postData, cancellationToken);


	/// <summary>
	/// Allow subclasses to provide their own implementations for <see cref="IRequestInvoker.Request{TResponse}"/> while reusing the more complex logic
	/// to create a response
	/// </summary>
	/// <param name="endpoint">An instance of <see cref="Endpoint"/> describing where to call out to</param>
	/// <param name="boundConfiguration">An instance of <see cref="BoundConfiguration"/> describing how to call out to</param>
	/// <param name="postData">The request body being sent to Elasticsearch</param>
	/// <param name="responseBody">An optional override for the response body</param>
	/// <param name="statusCode">The status code that the responses <see cref="TransportResponse.ApiCallDetails"/> should return</param>
	/// <param name="contentType">The content type of the response</param>
	public virtual TResponse BuildResponse<TResponse>(Endpoint endpoint, BoundConfiguration boundConfiguration, PostData? postData, byte[]? responseBody = null, int? statusCode = null,
		string? contentType = null)
		where TResponse : TransportResponse, new()
	{
    // If no response body was provided in the call to BuildResponse, use the response builder callback to create one.
    byte[] body;
    if (responseBody is null)
    {
      JsonNode requestBodyAsJson = PostDataToJson(postData, boundConfiguration.ConnectionSettings);
      string builtResponseBody = _responseBuilder(postData, requestBodyAsJson);
      body = Encoding.UTF8.GetBytes(builtResponseBody);
    }
    else
    {
      body = responseBody;
    }

		var data = postData;

		if (data is not null)
		{
			using var stream = boundConfiguration.MemoryStreamFactory.Create();
			if (boundConfiguration.HttpCompression)
			{
				using var zipStream = new GZipStream(stream, CompressionMode.Compress);
				data.Write(zipStream, boundConfiguration.ConnectionSettings, boundConfiguration.DisableDirectStreaming);
			}
			else
				data.Write(stream, boundConfiguration.ConnectionSettings, boundConfiguration.DisableDirectStreaming);
		}

		var sc = statusCode ?? _returnedStatusCode;
		Stream responseStream = body != null ? boundConfiguration.MemoryStreamFactory.Create(body) : boundConfiguration.MemoryStreamFactory.Create(EmptyBody);

		return ResponseFactory.Create<TResponse>(endpoint, boundConfiguration, postData, null, sc, Headers, responseStream, contentType ?? _returnedContentType ?? BoundConfiguration.DefaultContentType, body?.Length ?? 0, null, null);
	}

	/// <inheritdoc cref="BuildResponse{TResponse}"/>>
	public async Task<TResponse> BuildResponseAsync<TResponse>(Endpoint endpoint, BoundConfiguration boundConfiguration, PostData? postData, CancellationToken cancellationToken,
		byte[]? responseBody = null, int? statusCode = null, string? contentType = null)
		where TResponse : TransportResponse, new()
	{
    JsonNode requestBodyAsJson = PostDataToJson(postData, boundConfiguration.ConnectionSettings);
    string builtResponseBody = _responseBuilder(postData, requestBodyAsJson);

		var body = responseBody ?? System.Text.Encoding.UTF8.GetBytes(builtResponseBody);
		var data = postData;

		if (data is not null)
		{
			using var stream = boundConfiguration.MemoryStreamFactory.Create();

			if (boundConfiguration.HttpCompression)
			{
				using var zipStream = new GZipStream(stream, CompressionMode.Compress);
				await data.WriteAsync(zipStream, boundConfiguration.ConnectionSettings, boundConfiguration.DisableDirectStreaming, cancellationToken).ConfigureAwait(false);
			}
			else
			{
				await data.WriteAsync(stream, boundConfiguration.ConnectionSettings, boundConfiguration.DisableDirectStreaming, cancellationToken).ConfigureAwait(false);
			}
		}
		var sc = statusCode ?? _returnedStatusCode;

		Stream responseStream = body != null ? boundConfiguration.MemoryStreamFactory.Create(body) : boundConfiguration.MemoryStreamFactory.Create(EmptyBody);

		return await ResponseFactory
			.CreateAsync<TResponse>(endpoint, boundConfiguration, postData, null, sc, Headers, responseStream, contentType ?? _returnedContentType ?? BoundConfiguration.DefaultContentType, body?.Length ?? 0, null, null, cancellationToken)
			.ConfigureAwait(false);
	}



  /// <summary>
  /// Helper method to convert PostData to a JsonNode.
  /// </summary>
  /// <param name="postData">The PostData to convert.</param>
  /// <param name="config">The transport configuration.</param>
  /// <returns>A (possibly empty) JsonNode representing the PostData.</returns>
  private JsonNode PostDataToJson(PostData? postData, ITransportConfiguration config)
  {
    if (postData is null)
    {
      return new JsonObject();
    }

    using (var ms = new System.IO.MemoryStream())
    {
      postData.Write(ms, config, true);
      string json = System.Text.Encoding.UTF8.GetString(ms.ToArray());
      return JsonNode.Parse(json) ?? new JsonObject();
    }
  }

  /// <summary>
  /// Dispose the inner connection.  Note that this class doesn't actually have any resources to dispose, but we want to
  /// satisfy the IDisposable contract inherited from IRequestInvoker.
  /// </summary>
  public void Dispose()
  {
    ((IDisposable)_innerConnection).Dispose();
  }

}