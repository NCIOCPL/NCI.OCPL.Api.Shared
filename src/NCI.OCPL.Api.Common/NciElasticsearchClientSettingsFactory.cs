using System.Text.Json;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;

namespace NCI.OCPL.Api.Common;

/// <summary>
/// Factory for creating <see cref="ElasticsearchClientSettings"/> objects with our
/// custom settings.
/// </summary>
public static class NciElasticsearchClientSettingsFactory
{
  /// <summary>
  /// Creates a new instance of <see cref="ElasticsearchClientSettings"/> configured
  /// to use our custom JSON serialization settings, including
  /// <see cref="JsonNamingPolicy.SnakeCaseLower"/>.
  /// </summary>
  /// <param name="nodePool">The <see cref="NodePool"/> to use for the connection.</param>
  /// <param name="requestInvoker">
  /// The <see cref="IRequestInvoker"/> to use, or <c>null</c> to use the default.
  /// </param>
  /// <returns>A new instance of <see cref="ElasticsearchClientSettings"/>.</returns>
  public static ElasticsearchClientSettings Create(NodePool nodePool, IRequestInvoker requestInvoker = null)
  {
    // Callback to set options for the connection settings.
    static void ConfigureOptions(JsonSerializerOptions options)
    {
      // Deserialize from Elasticsearch using snake_case.
      options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    }

    return new ElasticsearchClientSettings(
      nodePool,
      requestInvoker,
      sourceSerializer: (defaultSerializer, settings) =>
        new DefaultSourceSerializer(settings, ConfigureOptions)
    );
  }
}
