using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NCI.OCPL.Api.Common;

namespace integration_test_harness
{
  /// <summary>
  /// Defines the configuration of the test harness API.
  /// </summary>
  public class Startup : NciStartupBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:NCI.OCPL.Api.Common.Startup"/> class.
    /// </summary>
    /// <param name="env">Env.</param>
    public Startup(IConfiguration configuration)
            : base(configuration) { }


    /*****************************
     * ConfigureServices methods *
     *****************************/

    /// <summary>
    /// Adds the configuration mappings.
    /// </summary>
    /// <param name="services">Services.</param>
    protected override void AddAdditionalConfigurationMappings(IServiceCollection services)
    {
      services.Configure<ESIndexOptions>(Configuration.GetSection("TestIndexOptions"));
    }

    /// <summary>
    /// Adds in the application specific services
    /// </summary>
    /// <param name="services">Services.</param>
    protected override void AddAppServices(IServiceCollection services)
    {
      services.AddTransient<IESAliasNameProvider>(p => {
        string alias = Configuration["TestIndexOptions:AliasName"];
        return new ESAliasNameProvider(){Name= alias};
      });

    }

    /*****************************
     *     Configure methods     *
     *****************************/

    /// <summary>
    /// Configure the specified app and env.
    /// </summary>
    /// <returns>The configure.</returns>
    /// <param name="app">App.</param>
    /// <param name="env">Env.</param>
    /// <param name="loggerFactory">Logger.</param>
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    protected override void ConfigureAppSpecific(IApplicationBuilder app, IWebHostEnvironment env)
    {
      return;
    }
  }
}
