using System;
using System.Text.Json;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;

namespace NCI.OCPL.Api.Common.Testing;

/// <summary>
/// Factory for creating <see cref="ElasticsearchClientSettings"/> objects with our
/// custom settings.
/// </summary>
/// <remarks>
/// Ideally, this factory would be in NCI.OCPL.Api.Common, and used by
/// NciStartupBase::ConfigureServices when creating the instance for the
/// application. Unfortunately, the default RequestInvoker is internal to
/// the Elastic.Clients.Elasticsearch assembly, so we cannot parameterize
/// that rather essential element.
/// </remarks>
public static class NciElasticsearchClientSettingsFactory
{
  /// <summary>
  /// Creates a new instance of <see cref="ElasticsearchClientSettings"/> with our
  /// custom settings.
  /// </summary>
  /// <param name="response">String containing a mock Elasticsearch response.</param>
  /// <param name="statusCode">Mock Elasticsearch HTTP status code.</param>
  /// <returns>A new instance of <see cref="ElasticsearchClientSettings"/>.</returns>
  public static ElasticsearchClientSettings Create(string response, int statusCode)
  {
    byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);

    InMemoryConnection conn = new InMemoryConnection(responseBytes, statusCode);

    return Create(conn);
  }

  /// <summary>
  /// Creates a new instance of <see cref="ElasticsearchClientSettings"/> with our
  /// custom settings.
  /// </summary>
  /// <remarks>
  /// This method is used when the caller needs to use an  <see cref="InMemoryConnection"/>
  /// (or other <see cref="InMemoryRequestInvoker"/>) with configuration beyond the response
  /// and status code (e.g. custom response headers).
  /// </remarks>
  /// <param name="connection">The <see cref="InMemoryRequestInvoker"/> to use for the connection.</param>
  /// <returns>A new instance of <see cref="ElasticsearchClientSettings"/>.</returns>
  public static ElasticsearchClientSettings Create(InMemoryRequestInvoker connection)
  {
    // While this has a URI, it does not matter, an InMemoryConnection never requests
    // from the server.
    var pool = new SingleNodePool(new Uri("http://localhost:9200"));

    // Callback to set options for the connection settings.
    static void ConfigureOptions(JsonSerializerOptions options)
    {
      // Deserialize from Elasticsearch using snake_case.
      options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    }

    return new ElasticsearchClientSettings(
      pool,
      connection,
      sourceSerializer: (defaultSerializer, settings) =>
        new DefaultSourceSerializer(settings, ConfigureOptions)
    );
  }

  /// <summary>
  /// Creates a new instance of <see cref="ElasticsearchClientSettings"/>.
  /// </summary>
  /// <param name="response">String containing a mock Elasticsearch response.</param>
  /// <param name="statusCode">Mock Elasticsearch HTTP status code.</param>
  /// <param name="callback">Callback to be invoked for retrieving call details.</param>
  /// <returns>A new instance of <see cref="ElasticsearchClientSettings"/>.</returns>
  public static ElasticsearchClientSettings Create(string response, int statusCode, Action<ApiCallDetails> callback)
  {
    return Create(response, statusCode)
      .DisableDirectStreaming()
      .OnRequestCompleted(callback);
  }

}