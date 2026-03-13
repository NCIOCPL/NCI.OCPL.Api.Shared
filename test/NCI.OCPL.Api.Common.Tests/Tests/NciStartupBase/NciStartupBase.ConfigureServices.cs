using System;
using System.IO;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;
using Xunit;

using Elastic.Clients.Elasticsearch;

namespace NCI.OCPL.Api.Common
{
  /// <summary>
  /// Tests for NciStartupBase.ConfigureServices
  /// </summary>
  public partial class NciStartupBaseTest
  {
    /// <summary>
    /// Verify that subclasses of NciStartupBase have a chance to add required services.
    /// </summary>
    [Fact]
    public void ConfigureServices_SubclassSetupServices()
    {
      string settings = @"
        {
          ""Elasticsearch"": {
            ""Servers"": ""http://localhost:9200"",
            ""Userid"": """",
            ""Password"":  """",
            ""MaximumRetries"": 5
          },
          ""Logging"": {
            ""LogLevel"": {
              ""Default"": ""Warning""
            }
          }
        }";

      ConfigurationBuilder builder = new ConfigurationBuilder();
      builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(settings)));
      IConfiguration config = builder.Build();

      Mock<NciStartupBase> mockStartup = new Mock<NciStartupBase>(config){ CallBase = true };

      mockStartup.Protected().Setup("AddAdditionalConfigurationMappings", Moq.Protected.ItExpr.IsAny<IServiceCollection>());
      mockStartup.Protected().Setup("AddAppServices", Moq.Protected.ItExpr.IsAny<IServiceCollection>());

      NciStartupBase startup = mockStartup.Object;

      IServiceCollection svcColl = new ServiceCollection();
      startup.ConfigureServices(svcColl);

      IServiceProvider serviceProvider = svcColl.BuildServiceProvider();

      // Verify the subclass had a chance to add services.
      mockStartup.Protected().Verify("AddAdditionalConfigurationMappings", Times.Once(), Moq.Protected.ItExpr.IsAny<IServiceCollection>());
      mockStartup.Protected().Verify("AddAppServices", Times.Once(), Moq.Protected.ItExpr.IsAny<IServiceCollection>());
    }

    /// <summary>
    /// Fail to get an Elasticsearch client instance when no servers are configured.
    /// </summary>
    [Fact]
    public void ConfigureServices_ElasticsearchBadConfiguration()
    {
      string appsettings = @"
      {
          ""Elasticsearch"": {
              ""Servers"": """",
              ""Userid"": """",
              ""Password"": """",
              ""MaximumRetries"": 5
          }
        }
      ";

      ConfigurationBuilder builder = new ConfigurationBuilder();
      builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appsettings)));
      IConfiguration config = builder.Build();

      Mock<NciStartupBase> mockStartup = new Mock<NciStartupBase>(config) { CallBase = true };

      NciStartupBase startup = mockStartup.Object;

      IServiceCollection svcColl = new ServiceCollection();
      startup.ConfigureServices(svcColl);

      IServiceProvider serviceProvider = svcColl.BuildServiceProvider();

      Assert.NotNull(serviceProvider);

      Exception ex = Assert.Throws<APIInternalException>(
        () => serviceProvider.GetService(typeof(ElasticsearchClient))
      );

      Assert.Equal("No servers configured", ex.Message);
    }

    /// <summary>
    /// Get an Elasticsearch client instance with a valid configuration.
    /// </summary>
    [Fact]
    public void ConfigureServices_ElasticsearchGoodConfiguration()
    {
      string appsettings = @"
      {
          ""Elasticsearch"": {
              ""Servers"": ""http://localhost:9200"",
              ""Userid"": """",
              ""Password"": """",
              ""MaximumRetries"": 5
          }
        }
      ";

      ConfigurationBuilder builder = new ConfigurationBuilder();
      builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appsettings)));
      IConfiguration config = builder.Build();

      Mock<NciStartupBase> mockStartup = new Mock<NciStartupBase>(config) { CallBase = true };

      NciStartupBase startup = mockStartup.Object;

      IServiceCollection svcColl = new ServiceCollection();
      startup.ConfigureServices(svcColl);

      IServiceProvider serviceProvider = svcColl.BuildServiceProvider();

      Assert.NotNull(serviceProvider);

      Object svc = serviceProvider.GetService(typeof(ElasticsearchClient));
      Assert.NotNull(svc);
      Assert.IsAssignableFrom<ElasticsearchClient>(svc);
    }

    /// <summary>
    /// Verify that we didn't forget to register the logging service.
    /// </summary>
    [Fact]
    public void ConfigureServices_GetLoggers()
    {
      string settings = @"
        {
          ""Elasticsearch"": {
            ""Servers"": ""http://localhost:9200"",
            ""Userid"": """",
            ""Password"":  """",
            ""MaximumRetries"": 5
          },
          ""Logging"": {
            ""LogLevel"": {
              ""Default"": ""Warning""
            }
          }
        }";

      ConfigurationBuilder builder = new ConfigurationBuilder();
      builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(settings)));
      IConfiguration config = builder.Build();

      Mock<NciStartupBase> mockStartup = new Mock<NciStartupBase>(config) { CallBase = true };

      mockStartup.Protected().Setup("AddAdditionalConfigurationMappings", Moq.Protected.ItExpr.IsAny<IServiceCollection>());

      NciStartupBase startup = mockStartup.Object;

      IServiceCollection svcColl = new ServiceCollection();
      startup.ConfigureServices(svcColl);

      IServiceProvider serviceProvider = svcColl.BuildServiceProvider();

      Object svc;

      svc = serviceProvider.GetService(typeof(ILogger<ElasticsearchClient>));
      Assert.NotNull(svc);
      Assert.IsAssignableFrom<Logger<ElasticsearchClient>>(svc);
    }

    /// <summary>
    /// Verify that HealthService is not reqistered if no alias name provider
    /// is registered.
    /// </summary>
    [Fact]
    public void ConfigureServices_HealthServiceNoAliasName()
    {
      string settings = @"
        {
          ""Elasticsearch"": {
            ""Servers"": ""http://localhost:9200"",
            ""Userid"": """",
            ""Password"":  """",
            ""MaximumRetries"": 5
          },
          ""Logging"": {
            ""LogLevel"": {
              ""Default"": ""Warning""
            }
          }
        }";

      ConfigurationBuilder builder = new ConfigurationBuilder();
      builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(settings)));
      IConfiguration config = builder.Build();

      Mock<NciStartupBase> mockStartup = new Mock<NciStartupBase>(config) { CallBase = true };

      mockStartup.Protected().Setup("AddAdditionalConfigurationMappings", Moq.Protected.ItExpr.IsAny<IServiceCollection>());

      NciStartupBase startup = mockStartup.Object;

      IServiceCollection svcColl = new ServiceCollection();
      startup.ConfigureServices(svcColl);

      IServiceProvider serviceProvider = svcColl.BuildServiceProvider();

      Object svc;

      // No IESAliasNameProvider handler was registered, so IHealthCheckService is not supposed to be available.
      svc = serviceProvider.GetService(typeof(IHealthCheckService));

      Assert.Null(svc);
    }

    /// <summary>
    /// Verify that HealthService is reqistered when an alias name provider
    /// is registered.
    /// </summary>
    [Fact]
    public void ConfigureServices_HealthServiceWithAliasName()
    {
      string settings = @"
        {
          ""Elasticsearch"": {
            ""Servers"": ""http://localhost:9200"",
            ""Userid"": """",
            ""Password"":  """",
            ""MaximumRetries"": 5
          },
          ""Logging"": {
            ""LogLevel"": {
              ""Default"": ""Warning""
            }
          }
        }";

      ConfigurationBuilder builder = new ConfigurationBuilder();
      builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(settings)));
      IConfiguration config = builder.Build();

      Mock<IESAliasNameProvider> mockNameProvider = new Mock<IESAliasNameProvider>();
      mockNameProvider.Setup( p => p.Name).Returns("testAlias");


      Mock<NciStartupBase> mockStartup = new Mock<NciStartupBase>(config) { CallBase = true };

      mockStartup.Protected().Setup("AddAdditionalConfigurationMappings", Moq.Protected.ItExpr.IsAny<IServiceCollection>());

      // Set up a handler for IESAliasNameProvider.
      mockStartup.Protected().Setup("AddAppServices", Moq.Protected.ItExpr.IsAny<IServiceCollection>())
        .Callback((IServiceCollection svc) => svc.AddTransient<IESAliasNameProvider>( p => mockNameProvider.Object));

      NciStartupBase startup = mockStartup.Object;

      IServiceCollection svcColl = new ServiceCollection();
      startup.ConfigureServices(svcColl);

      IServiceProvider serviceProvider = svcColl.BuildServiceProvider();

      Object svc;

      // No IESAliasNameProvider handler was registered, so IHealthCheckService is not supposed to be available.
      svc = serviceProvider.GetService(typeof(IHealthCheckService));

      Assert.NotNull(svc);
      Assert.IsType<ESHealthCheckService>(svc);

      mockNameProvider.VerifyGet(p => p.Name);
    }
  }
}