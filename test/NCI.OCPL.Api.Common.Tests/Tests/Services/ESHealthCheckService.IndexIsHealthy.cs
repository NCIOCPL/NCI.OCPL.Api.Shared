using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace NCI.OCPL.Api.Common.Tests
{
  /// <summary>
  /// Unit tests for the <see cref="ESHealthCheckService.IndexIsHealthy"/> method.
  /// </summary>
  public partial class ESHealthCheckServiceTest
  {
    /// <summary>
    /// Verify that <see cref="ESHealthCheckService.IndexIsHealthy"/> returns true
    /// when the cluster status is green.
    /// </summary>
    [Fact]
    public async Task IndexIsHealthy_GreenStatus_ReturnsTrue()
    {
      // Arrange
      string aliasName = "test-alias";
      ElasticsearchClient client = GetMockElasticClient(GetGreenStatusResponse());
      Mock<IESAliasNameProvider> mockAliasProvider = new Mock<IESAliasNameProvider>();
      mockAliasProvider.Setup(p => p.Name).Returns(aliasName);
      Mock<ILogger<ESHealthCheckService>> mockLogger = new Mock<ILogger<ESHealthCheckService>>();

      ESHealthCheckService service = new ESHealthCheckService(
        client,
        mockAliasProvider.Object,
        mockLogger.Object
      );

      // Act
      bool result = await service.IndexIsHealthy();

      // Assert
      Assert.True(result);
    }

    /// <summary>
    /// Verify that <see cref="ESHealthCheckService.IndexIsHealthy"/> returns true
    /// when the cluster status is yellow.
    /// </summary>
    [Fact]
    public async Task IndexIsHealthy_YellowStatus_ReturnsTrue()
    {
      // Arrange
      string aliasName = "test-alias";
      ElasticsearchClient client = GetMockElasticClient(GetYellowStatusResponse());
      Mock<IESAliasNameProvider> mockAliasProvider = new Mock<IESAliasNameProvider>();
      mockAliasProvider.Setup(p => p.Name).Returns(aliasName);
      Mock<ILogger<ESHealthCheckService>> mockLogger = new Mock<ILogger<ESHealthCheckService>>();

      ESHealthCheckService service = new ESHealthCheckService(
        client,
        mockAliasProvider.Object,
        mockLogger.Object
      );

      // Act
      bool result = await service.IndexIsHealthy();

      // Assert
      Assert.True(result);
    }

    /// <summary>
    /// Verify that <see cref="ESHealthCheckService.IndexIsHealthy"/> returns false
    /// when the cluster status is red.
    /// </summary>
    [Fact]
    public async Task IndexIsHealthy_RedStatus_ReturnsFalse()
    {
      // Arrange
      string aliasName = "test-alias";
      ElasticsearchClient client = GetMockElasticClient(GetRedStatusResponse());
      Mock<IESAliasNameProvider> mockAliasProvider = new Mock<IESAliasNameProvider>();
      mockAliasProvider.Setup(p => p.Name).Returns(aliasName);
      Mock<ILogger<ESHealthCheckService>> mockLogger = new Mock<ILogger<ESHealthCheckService>>();

      ESHealthCheckService service = new ESHealthCheckService(
        client,
        mockAliasProvider.Object,
        mockLogger.Object
      );

      // Act
      bool result = await service.IndexIsHealthy();

      // Assert
      Assert.False(result);
    }

    /// <summary>
    /// Verify that <see cref="ESHealthCheckService.IndexIsHealthy"/> returns false
    /// when the Elasticsearch request returns an error status code.
    /// </summary>
    [Theory]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(503)]
    public async Task IndexIsHealthy_ErrorStatusCode_ReturnsFalse(int statusCode)
    {
      // Arrange
      string aliasName = "test-alias";
      ElasticsearchClient client = GetErrorElasticClient(statusCode);
      Mock<IESAliasNameProvider> mockAliasProvider = new Mock<IESAliasNameProvider>();
      mockAliasProvider.Setup(p => p.Name).Returns(aliasName);
      Mock<ILogger<ESHealthCheckService>> mockLogger = new Mock<ILogger<ESHealthCheckService>>();

      ESHealthCheckService service = new ESHealthCheckService(
        client,
        mockAliasProvider.Object,
        mockLogger.Object
      );

      // Act
      bool result = await service.IndexIsHealthy();

      // Assert
      Assert.False(result);
    }

    /// <summary>
    /// Verify that <see cref="ESHealthCheckService.IndexIsHealthy"/> returns false
    /// when an exception is thrown during the health check.
    /// </summary>
    [Fact]
    public async Task IndexIsHealthy_ExceptionThrown_ReturnsFalse()
    {
      // Arrange
      string aliasName = "test-alias";
      ElasticsearchClient client = GetExceptionThrowingClient();
      Mock<IESAliasNameProvider> mockAliasProvider = new Mock<IESAliasNameProvider>();
      mockAliasProvider.Setup(p => p.Name).Returns(aliasName);
      Mock<ILogger<ESHealthCheckService>> mockLogger = new Mock<ILogger<ESHealthCheckService>>();

      ESHealthCheckService service = new ESHealthCheckService(
        client,
        mockAliasProvider.Object,
        mockLogger.Object
      );

      // Act
      bool result = await service.IndexIsHealthy();

      // Assert
      Assert.False(result);
    }

    #region Helper Methods

    /// <summary>
    /// Gets a mock ElasticsearchClient that returns the specified response body.
    /// </summary>
    private static ElasticsearchClient GetMockElasticClient(byte[] responseBody)
    {
      var pool = new SingleNodePool(new Uri("http://localhost:9200"));

      var headers = new Dictionary<string, IEnumerable<string>>
      {
        { "x-elastic-product", new[] { "Elasticsearch" } }
      };

      InMemoryRequestInvoker conn = new InMemoryRequestInvoker(responseBody, headers: headers);
      var connectionSettings = new ElasticsearchClientSettings(pool, conn);

      return new ElasticsearchClient(connectionSettings);
    }

    /// <summary>
    /// Gets a mock ElasticsearchClient that returns an error status code.
    /// </summary>
    private static ElasticsearchClient GetErrorElasticClient(int statusCode)
    {
      var pool = new SingleNodePool(new Uri("http://localhost:9200"));
      byte[] responseBody = Array.Empty<byte>();

      var headers = new Dictionary<string, IEnumerable<string>>
      {
        { "x-elastic-product", new[] { "Elasticsearch" } }
      };

      InMemoryRequestInvoker conn = new InMemoryRequestInvoker(responseBody, statusCode: statusCode, headers: headers);
      var connectionSettings = new ElasticsearchClientSettings(pool, conn);

      return new ElasticsearchClient(connectionSettings);
    }

    /// <summary>
    /// Gets a mock ElasticsearchClient that throws an exception.
    /// </summary>
    private static ElasticsearchClient GetExceptionThrowingClient()
    {
      var pool = new SingleNodePool(new Uri("http://localhost:9200"));

      var headers = new Dictionary<string, IEnumerable<string>>
      {
        { "x-elastic-product", new[] { "Elasticsearch" } }
      };

      // Use an empty response with a failure status to simulate connection issues
      InMemoryRequestInvoker conn = new InMemoryRequestInvoker(
        Array.Empty<byte>(),
        statusCode: 0,
        exception: new Exception("Connection failed"),
        headers: headers
      );
      var connectionSettings = new ElasticsearchClientSettings(pool, conn);

      return new ElasticsearchClient(connectionSettings);
    }

    /// <summary>
    /// Returns a JSON response representing a green cluster health status.
    /// </summary>
    private static byte[] GetGreenStatusResponse()
    {
      string json = @"{
        ""cluster_name"" : ""docker - cluster"",
        ""status"" : ""green"",
        ""timed_out"" : false,
        ""number_of_nodes"" : 1,
        ""number_of_data_nodes"" : 1,
        ""active_primary_shards"" : 1,
        ""active_shards"" : 1,
        ""relocating_shards"" : 0,
        ""initializing_shards"" : 0,
        ""unassigned_shards"" : 1,
        ""unassigned_primary_shards"" : 0,
        ""delayed_unassigned_shards"" : 0,
        ""number_of_pending_tasks"" : 0,
        ""number_of_in_flight_fetch"" : 0,
        ""task_max_waiting_in_queue_millis"" : 0,
        ""active_shards_percent_as_number"" : 50.0
      }";
      return System.Text.Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// Returns a JSON response representing a yellow cluster health status.
    /// </summary>
    private static byte[] GetYellowStatusResponse()
    {
      string json = @"{
        ""cluster_name"" : ""docker - cluster"",
        ""status"" : ""yellow"",
        ""timed_out"" : false,
        ""number_of_nodes"" : 1,
        ""number_of_data_nodes"" : 1,
        ""active_primary_shards"" : 1,
        ""active_shards"" : 1,
        ""relocating_shards"" : 0,
        ""initializing_shards"" : 0,
        ""unassigned_shards"" : 1,
        ""unassigned_primary_shards"" : 0,
        ""delayed_unassigned_shards"" : 0,
        ""number_of_pending_tasks"" : 0,
        ""number_of_in_flight_fetch"" : 0,
        ""task_max_waiting_in_queue_millis"" : 0,
        ""active_shards_percent_as_number"" : 50.0
      }";
      return System.Text.Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// Returns a JSON response representing a red cluster health status.
    /// </summary>
    private static byte[] GetRedStatusResponse()
    {
      string json = @"{
        ""cluster_name"" : ""docker - cluster"",
        ""status"" : ""red"",
        ""timed_out"" : false,
        ""number_of_nodes"" : 1,
        ""number_of_data_nodes"" : 1,
        ""active_primary_shards"" : 1,
        ""active_shards"" : 1,
        ""relocating_shards"" : 0,
        ""initializing_shards"" : 0,
        ""unassigned_shards"" : 1,
        ""unassigned_primary_shards"" : 0,
        ""delayed_unassigned_shards"" : 0,
        ""number_of_pending_tasks"" : 0,
        ""number_of_in_flight_fetch"" : 0,
        ""task_max_waiting_in_queue_millis"" : 0,
        ""active_shards_percent_as_number"" : 50.0
      }";
      return System.Text.Encoding.UTF8.GetBytes(json);
    }

    #endregion
  }
}
