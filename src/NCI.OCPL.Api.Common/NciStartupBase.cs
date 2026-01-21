using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NCI.OCPL.Api.Common.Models.Options;

using Nest;
using Nest.JsonNetSerializer;
using Elasticsearch.Net;


namespace NCI.OCPL.Api.Common
{
  /// <summary>
  /// Defines the base configuration for API microservices.
  /// </summary>
  public abstract class NciStartupBase
  {
    /// <summary>
    ///
    /// </summary>
    /// <param name="configuration"></param>
    public NciStartupBase(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    /// <value>The configuration.</value>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <returns>The services.</returns>
    /// <param name="services">Services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      /*
        Do we still need this? Or is this .Net 1.1 code?
      */
      services.AddLogging();

      //Turn on the OptionsManager that supports IOptions
      services.AddOptions();


      //Add Configuration mappings.
      services.Configure<ElasticsearchOptions>(Configuration.GetSection("Elasticsearch"));
      AddAdditionalConfigurationMappings(services);

      /*
        Do we still need this? Or is this Best Bets code which never should
        have been in the base class to begin with?
      */
      //Add HttpClient singleton, which is used by the display service.
      services.AddSingleton<HttpClient, HttpClient>();


      // This will inject an IElasticClient using our configuration into any
      // controllers that take an IElasticClient parameter into its constructor.
      //
      // AddSingleton means that only once instance of the ElasticClient will be
      // created for the lifetime of the application.  This is the recommended
      // approach per the ElasticSearch documentation. (Creating multiple clients
      // can cause port exhaustion.)
      services.AddSingleton<IElasticClient>(p =>
      {

        // Get the ElasticSearch credentials.
        string username = Configuration["Elasticsearch:Userid"];
        string password = Configuration["Elasticsearch:Password"];

        //Get the ElasticSearch servers that we will be connecting to.
        List<Uri> uris = GetElasticsearchServerUriList();

        // Create the connection pool, the SniffingConnectionPool will
        // keep tabs on the health of the servers in the cluster and
        // probe them to ensure they are healthy.  This is how we handle
        // redundancy and load balancing.
        var connectionPool = new SniffingConnectionPool(uris);

        //Return a new instance of an ElasticClient with our settings
        ConnectionSettings settings = new ConnectionSettings(connectionPool, sourceSerializer: JsonNetSerializer.Default);

        //Let's only try and use credentials if the username is set.
        if (!string.IsNullOrWhiteSpace(username))
        {
          settings.BasicAuthentication(username, password);
        }

        return new ElasticClient(settings);
      });

      //Add in Application specific services
      AddAppServices(services);

      // Don't set up the ESHealthCheck service unless the application registers
      // a handler to create instances of IESAliasName. Without this check, an
      // exception will occur during initialization because there's no way to resolve
      // the dependency. Doing it this way allows us to make it optional.
      if(services.Any(svc => svc.ServiceType == typeof(IESAliasNameProvider)))
      {
        services.AddTransient<IHealthCheckService, ESHealthCheckService>();
      }




      // Create CORS policies.
      services.AddCors();

      // Make the application's routes available.
      services.AddControllers()
        .AddNewtonsoftJson();

      // Enable Swagger
      // This creates the Swagger Json
      services.AddOpenApiDocument(config =>
      {
        if (!string.IsNullOrEmpty(Configuration["NSwag:Title"]))
        {
          config.Title = Configuration["NSwag:Title"];
        }

        if (!string.IsNullOrEmpty(Configuration["NSwag:Description"]))
        {
          config.Description = Configuration["NSwag:Description"];
        }
      });
    }

    /// <summary>
    /// Configure the app.
    /// </summary>
    /// <param name="app">App.</param>
    /// <param name="env">Hosting enviroment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseStaticFiles();
      app.UseRouting();

      // Enable the Swagger UI middleware and the Swagger generator
      // This serves the Swagger.json
      app.UseOpenApi(settings =>
      {
        settings.PostProcess = (document, request) =>
        {
          document.Servers.Clear();

          // Force the document's base path to match the request.
          // This is important as the request that generated the document won't
          // match when IIS is acting as a proxy.
          document.BasePath = request.PathBase;
        };
      });
      // This serves the Swagger UI
      app.UseSwaggerUi3(settings =>
      {
        // Set this as the default path.
        settings.Path = "";
      });

      // Allow use from anywhere.
      app.UseCors(builder => builder.AllowAnyOrigin());

      // This is equivelant to the old Global.asax OnError event handler.
      // It will handle any unhandled exception and return a status code to the
      // caller.  IF the error is of type APIErrorException then we will also return
      // a message along with the status code.  (Otherwise we )
      app.UseExceptionHandler(errorApp =>
      {
        errorApp.Run(async context =>
        {
          context.Response.StatusCode = 500; // or another Status accordingly to Exception Type
          context.Response.ContentType = "application/json";

          var error = context.Features.Get<IExceptionHandlerFeature>();

          if (error != null)
          {
            var ex = error.Error;

            //Unhandled exceptions may not be sanitized, so we will not
            //display the issue.
            string message = "Errors have occurred.  Type: " + ex.GetType().ToString();

            //Our own exceptions should be sanitized enough.
            if (ex is APIErrorException)
            {
              context.Response.StatusCode = ((APIErrorException)ex).HttpStatusCode;
              message = ex.Message;
            }

            byte[] contents = Encoding.UTF8.GetBytes(new ErrorMessage()
            {
              Message = message
            }.ToString());

            // THIS IS A HACK!!
            // When the pull request that fixes the timing of setting the CORS header (https://github.com/aspnet/CORS/pull/163) goes through,
            // we should remove this and test to see if it works without the hack.
            if (context.Request.Headers.ContainsKey("Origin"))
            {
              context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            }

            await context.Response.Body.WriteAsync(contents, 0, contents.Length);
          }
        });
      });

      //Call derrived class' method for configuring additional
      //functionality
      ConfigureAppSpecific(app, env);

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });

    }

    /*****************************
     * ConfigureServices methods *
     *****************************/

    /// <summary>
    /// Adds the configuration mappings.
    /// </summary>
    /// <param name="services">Services.</param>
    protected abstract void AddAdditionalConfigurationMappings(IServiceCollection services);

    /// <summary>
    /// Adds in the application specific services
    /// </summary>
    /// <param name="services">Services.</param>
    protected abstract void AddAppServices(IServiceCollection services);

    /*****************************
     *     Configure methods     *
     *****************************/

    /// <summary>
    /// Configure the specified app and env.
    /// </summary>
    /// <returns>The configure.</returns>
    /// <param name="app">App.</param>
    /// <param name="env">Env.</param>
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    protected abstract void ConfigureAppSpecific(IApplicationBuilder app, IWebHostEnvironment env);


    /// <summary>
    /// Retrieves a list of Elasticsearch server URIs from the configuration's Elasticsearch:Servers setting.
    /// </summary>
    /// <returns>Returns a list of one or more Uri objects representing the configured set of Elasticsearch servers</returns>
    /// <remarks>
    /// The configuration's Elasticsearch:Servers property is required to contain URIs for one or more Elasticsearch servers.
    /// Each URI must include a protocol (http or https), a server name, and optionally, a port number.
    /// Multiple URIs are separated by a comma.  (e.g. "https://fred:9200, https://george:9201, https://ginny:9202")
    ///
    /// Throws ConfigurationException if no servers are configured.
    ///
    /// Throws UriFormatException if any of the configured server URIs are not formatted correctly.
    /// </remarks>
    private List<Uri> GetElasticsearchServerUriList()
    {
      List<Uri> uris = new List<Uri>();

      string serverList = Configuration["Elasticsearch:Servers"];
      if (!String.IsNullOrWhiteSpace(serverList))
      {
        // Convert the list of servers into a list of Uris.
        string[] names = serverList.Split(',');
        uris.AddRange(names.Select(server => new Uri(server)));
      }
      else
      {
        throw new APIInternalException("No servers configured");
      }

      return uris;
    }

  }
}