NCI.OCPL.Api.Shared

Common code for the CancerGov APIs.

# Consuming these packages

In order to consume these packages, you must configure the .Net SDK to retrieve NuGet packages from GitHub.

**NOTE:** These packages are presently built-on [version 3.1 of the .Net Core SDK](https://dotnet.microsoft.com/download/dotnet/3.1). This is subject to change.

1. On GitHub, create a [Personal Access Token](https://github.com/settings/tokens/) with a descriptive name such as "Retrieve NuGet Packages."
2. Assign the token the `packages:read` scope and save it.
3. Copy the token's value.
4. From the command prompt, and outside any .Net project's directory tree, run the following command (all one line), substituting your username and the token value:
    ```bash
    dotnet nuget add source https://nuget.pkg.github.com/nciocpl/index.json --name github --username <YOUR_GITHUB_USERNAME> --password <THE_TOKEN_VALUE> --store-password-in-clear-text
    ```

## Shared components

1. In the .Net project's directory, run the command:
    ```bash
    dotnet add package nci.ocpl.api.common
    ```
2. Replace the contents of Program.cs with
    ```csharp
      using Microsoft.AspNetCore.Hosting;
      using Microsoft.Extensions.Hosting;

      using NCI.OCPL.Api.Common;

      namespace my_namespace
      {
          public class Program : NciApiProgramBase
          {
              public static void Main(string[] args)
              {
                  CreateHostBuilder<Startup>(args).Build().Run();
              }
          }
      }
    ```
3. In Startup.cs
   - Add `using NCI.OCPL.Api.Common;`
   - Change the class declaration to:
      ```csharp
      public class Startup : NciStartupBase
      ```
  - Replace the constructor with:
     ```csharp
      public Startup(IConfiguration configuration)
              : base(configuration) { }
     ```
  - Remove the `ConfigureServices()` and `Configure()` methods.
  - Add these required overrides:
      - See `test/integration-test-harness/Startup.cs` for sample code.
        ```csharp
        protected override void AddAdditionalConfigurationMappings(IServiceCollection services)
        {}

        protected override void AddAppServices(IServiceCollection services)
        {}

        protected override void ConfigureAppSpecific(IApplicationBuilder app, IWebHostEnvironment env)
        {}
        ```

## ESHealthCheckService

**⚠️ Important ⚠️** In order to use the `ESHealthCheckService` with dependency injection, you must register a handler for `IESAliasNameProvider` in `AddAppServices`. As in:

```csharp
    protected override void AddAppServices(IServiceCollection services)
    {
      services.AddTransient<IESAliasNameProvider>(p => {
        string alias = Configuration["TestIndexOptions:AliasName"];
        return new ESAliasNameProvider(){Name= alias};
      });
    }
```