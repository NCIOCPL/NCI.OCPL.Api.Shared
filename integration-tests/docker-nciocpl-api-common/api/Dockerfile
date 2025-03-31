FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
COPY ./src /app/src
COPY ./test /app/test
COPY ./nci.ocpl.api.common.sln /app/app.sln
WORKDIR /app
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
COPY ./integration-tests/docker-nciocpl-api-common/api/build/appsettings.inttest.json .
COPY ./integration-tests/docker-nciocpl-api-common/api/build/hosting.json .
## This does not need to wait for the loader or other resources.
## Any integration tests will need to wait for the API to report being healthy
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT=inttest
ENV ASPNETCORE_URLS="http://*:5000"
ENTRYPOINT ["dotnet", "integration-test-harness.dll"]