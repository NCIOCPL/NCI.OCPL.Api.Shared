using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.IndexManagement;
using System.Linq;



namespace NCI.OCPL.Api.Common
{
  /// <summary>
  /// This class defines a service that can be used to determine whether an Elasticsearch cluster
  /// is in a healthy condition.
  /// </summary>
  public class ESHealthCheckService : IHealthCheckService
  {
    private ElasticsearchClient _elasticClient;
    private string _aliasName;
    private readonly ILogger<ESHealthCheckService> _logger;

    /// <summary>
    /// Creates a new instance of a ESHealthCheckService
    /// </summary>
    /// <param name="client">The client to be used for connections</param>
    /// <param name="aliasNamer">The name of the alias to check</param>
    /// <param name="logger">Logger instance.</param>
    public ESHealthCheckService(ElasticsearchClient client,
            IESAliasNameProvider aliasNamer,
            ILogger<ESHealthCheckService> logger)
    {
      _elasticClient = client;
      _aliasName = aliasNamer.Name;
      _logger = logger;
    }

    /// <summary>
    /// True if ESHealthCheckService if the index is usable and the cluster health status is either green or yellow.
    /// </summary>
    public async Task<bool> IndexIsHealthy()
    {
      // Use the cluster health API to verify that the index is functioning.
      // Maps to https://localhost:9200/_cluster/health/bestbets?pretty (or other server)
      //
      // References:
      // https://www.elastic.co/guide/en/elasticsearch/reference/master/cluster-health.html
      // https://github.com/elastic/elasticsearch/blob/master/rest-api-spec/src/main/resources/rest-api-spec/api/cluster.health.json#L20

      try
      {
        Task<HealthResponse> clusterHealthTask = _elasticClient.Cluster.HealthAsync(new HealthRequest(_aliasName));
        Task<ResolveIndexResponse> indexHealthTask = _elasticClient.Indices.ResolveIndexAsync(new ResolveIndexRequest(_aliasName));

        await Task.WhenAll(clusterHealthTask, indexHealthTask);

        HealthResponse clusterHealthResponse = await clusterHealthTask;
        ResolveIndexResponse indexHealthResponse = await indexHealthTask;

        if (!clusterHealthResponse.IsValidResponse)
        {
          _logger.LogError($"Error checking ElasticSearch health for {_aliasName}.");
          _logger.LogError($"Returned debug info: {clusterHealthResponse.DebugInformation}.");
        }
        else if (!indexHealthResponse.IsValidResponse)
        {
          _logger.LogError($"Error checking ElasticSearch health for {_aliasName}.");
          _logger.LogError($"Returned debug info: {indexHealthResponse.DebugInformation}.");
        }
        else
        {
          // The cluster has at least one healthy shard, there is exactly one backing index, and it is open (usable).
          if ((clusterHealthResponse.Status == HealthStatus.Green || clusterHealthResponse.Status == HealthStatus.Yellow)
              && indexHealthResponse.Indices.Count == 1
              && indexHealthResponse.Indices.Any( i => i.Attributes.Contains("open")))
          {
            //This is the only condition that will return true
            return true;
          }
          else
          {
            _logger.LogError($"Alias {_aliasName} status is not good");
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error checking ElasticSearch health for {_aliasName}.");
      }
      return false;
    }
  }
}