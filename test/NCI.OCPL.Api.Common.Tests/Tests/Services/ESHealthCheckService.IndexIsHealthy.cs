using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Moq;
using Xunit;

using NCI.OCPL.Api.Common.Testing;

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
      var settings = TestingElasticsearchClientSettingsFactory.Create(GreenStatusResponse, 200);
      var client = new ElasticsearchClient(settings);
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
      var settings = TestingElasticsearchClientSettingsFactory.Create(YellowStatusResponse, 200);
      var client = new ElasticsearchClient(settings);
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
      var settings = TestingElasticsearchClientSettingsFactory.Create(RedStatusResponse, 200);
      var client = new ElasticsearchClient(settings);
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
      var settings = TestingElasticsearchClientSettingsFactory.Create("", statusCode);
      var client = new ElasticsearchClient(settings);
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

      var conn = new InMemoryRequestInvoker(new byte[0], 500, new Exception("Simulated connection failure"));

      var settings = TestingElasticsearchClientSettingsFactory.Create(conn);
      var client = new ElasticsearchClient(settings);
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

    #region Helpers

    /// <summary>
    /// JSON response representing a green cluster health status.
    /// </summary>
    private const string GreenStatusResponse =
      @"{
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

    /// <summary>
    /// JSON response representing a yellow cluster health status.
    /// </summary>
    private const string YellowStatusResponse =
      @"{
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

    /// <summary>
    /// JSON response representing a red cluster health status.
    /// </summary>
    private const string RedStatusResponse =
      @"{
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

    #endregion
  }
}
