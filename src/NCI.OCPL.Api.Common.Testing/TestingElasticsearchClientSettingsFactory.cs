using System;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace NCI.OCPL.Api.Common.Testing;

/// <summary>
/// Factory for creating <see cref="ElasticsearchClientSettings"/> objects
/// configured for use in tests with in-memory connections.
/// </summary>
public static class TestingElasticsearchClientSettingsFactory
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
    // While this has a URI, it does not matter, we are explicitly using an
    // im-memory connection to avoid calling a real Elasticsearch instance.
    var pool = new SingleNodePool(new Uri("http://localhost:9200"));

    return NciElasticsearchClientSettingsFactory.Create(pool, connection);
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