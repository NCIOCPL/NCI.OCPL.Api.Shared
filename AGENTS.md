## Project Overview

This project builds NuGet packages used across NCIOCPL's web apis.  A sample program using these components is located in test/integration-test-harness.

## Tech Stack

- **Runtime:** .NET 8 (global.json)
- **Database:** Elasticsearch 8.19.12
- **Serialization:** System.Text.Json
- **DI:** Microsoft.Extensions.DependencyInjection
- **Testing:** Xunit (2.x), Moq (4.16), Microsoft.NET.Test.Sdk (16.10)

## Build and Test

```bash
# Build
dotnet build nci.ocpl.api.common.sln -c Debug

# Test
dotnet test

# Run specific test
dotnet test --filter "<fully-qualified-test-name>"
```

Integration tests are in the integration-tests folder.  Refer to integration-tests/README.md for additional information.

## Coding Standards

- Code must compile without warnings.
- Do not leave unused code, dead branches, or unnecessary `using` statements.
- Add comments only when they explain intent, non-obvious decisions, or important caveats. Do not add comments that merely restate the code.

### Namespace and Using Style

- All classes in src/NCI.OCPL.Api.Common belong to the `NCI.OCPL.Api.Common` namespace.
- All classes in src/NCI.OCPL.Api.Common.Testing belong to the `NCI.OCPL.Api.Common.Testing` namespace (exceptions: NullLogger.cs and NullLoggerOfT.cs).
- Organize and alphabetically sort `using` statements into these groups, in this order:
  1. `System.*`
  2. `Microsoft.*`
  3. Third-party packages
  4. Namespaces from this solution
- Groups of `using` statements are separated by a single blank line.

### Elasticsearch
- Use the `Elastic.Clients.Elasticsearch.ElasticsearchClient` Elasticsearch 8 client.
- Do **NOT** introduce usage of older Elasticsearch clients such as `Nest`.
- Prefer modern, strongly typed client usage where practical and consistent with the repository.
