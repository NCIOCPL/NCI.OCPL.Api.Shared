using System;
using System.IO;
using System.Text;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  /// <summary>
  /// Tests for <see cref="NciElasticsearchClientSettingsFactory"/>.
  /// </summary>
  public class NciElasticsearchClientSettingsFactoryTest
  {
    /// <summary>
    /// Verify that Create returns a non-null ElasticsearchClientSettings instance.
    /// </summary>
    [Fact]
    public void Create_ReturnsNonNullSettings()
    {
      ElasticsearchClientSettings settings = NciElasticsearchClientSettingsFactory.Create("{}", 200);

      Assert.NotNull(settings);
    }

    /// <summary>
    /// Verify that the returned settings can be used to construct an ElasticsearchClient.
    /// </summary>
    [Fact]
    public void Create_SettingsProduceValidClient()
    {
      ElasticsearchClientSettings settings = NciElasticsearchClientSettingsFactory.Create("{}", 200);
      var client = new ElasticsearchClient(settings);

      Assert.NotNull(client);
    }

    /// <summary>
    /// Verify that the source serializer uses snake_case naming policy for serialization.
    /// </summary>
    [Fact]
    public void Create_UsesSnakeCaseNamingPolicy()
    {
      ElasticsearchClientSettings settings = NciElasticsearchClientSettingsFactory.Create("{}", 200);
      var client = new ElasticsearchClient(settings);

      // Serialize a test object with a PascalCase property and verify snake_case output.
      using var stream = new MemoryStream();
      client.SourceSerializer.Serialize(new TestDocument { MyProperty = "value" }, stream);
      string json = Encoding.UTF8.GetString(stream.ToArray());

      Assert.Contains("my_property", json);
      Assert.DoesNotContain("MyProperty", json);
    }

    /// <summary>
    /// Verify that deserialization correctly maps snake_case JSON to PascalCase properties.
    /// </summary>
    [Fact]
    public void Create_DeserializesSnakeCaseProperties()
    {
      ElasticsearchClientSettings settings = NciElasticsearchClientSettingsFactory.Create("{}", 200);
      var client = new ElasticsearchClient(settings);

      string json = "{\"my_property\":\"hello\"}";
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
      TestDocument result = client.SourceSerializer.Deserialize<TestDocument>(stream);

      Assert.Equal("hello", result.MyProperty);
    }

    /// <summary>
    /// Verify that Create with a callback returns a non-null ElasticsearchClientSettings instance.
    /// </summary>
    [Fact]
    public void Create_WithCallback_ReturnsNonNullSettings()
    {
      ElasticsearchClientSettings settings = NciElasticsearchClientSettingsFactory.Create("{}", 200, _ => { });

      Assert.NotNull(settings);
    }

    /// <summary>
    /// Verify that the callback overload produces settings that can construct a valid ElasticsearchClient.
    /// </summary>
    [Fact]
    public void Create_WithCallback_SettingsProduceValidClient()
    {
      ElasticsearchClientSettings settings = NciElasticsearchClientSettingsFactory.Create("{}", 200, _ => { });
      var client = new ElasticsearchClient(settings);

      Assert.NotNull(client);
    }

    /// <summary>
    /// Verify that Create with an InMemoryRequestInvoker returns a non-null ElasticsearchClientSettings instance.
    /// </summary>
    [Fact]
    public void Create_WithConnection_ReturnsNonNullSettings()
    {
      using var conn = new InMemoryConnection(Array.Empty<byte>(), 200);

      ElasticsearchClientSettings settings = NciElasticsearchClientSettingsFactory.Create(conn);

      Assert.NotNull(settings);
    }

    /// <summary>
    /// Verify that settings created from an InMemoryRequestInvoker produce a valid ElasticsearchClient.
    /// </summary>
    [Fact]
    public void Create_WithConnection_SettingsProduceValidClient()
    {
      using var conn = new InMemoryConnection(Array.Empty<byte>(), 200);

      ElasticsearchClientSettings settings = NciElasticsearchClientSettingsFactory.Create(conn);
      var client = new ElasticsearchClient(settings);

      Assert.NotNull(client);
    }

    /// <summary>
    /// Verify that the source serializer uses snake_case naming policy for serialization
    /// when settings are created from an InMemoryRequestInvoker.
    /// </summary>
    [Fact]
    public void Create_WithConnection_UsesSnakeCaseNamingPolicy()
    {
      using var conn = new InMemoryConnection(Array.Empty<byte>(), 200);
      var client = new ElasticsearchClient(NciElasticsearchClientSettingsFactory.Create(conn));

      using var stream = new MemoryStream();
      client.SourceSerializer.Serialize(new TestDocument { MyProperty = "value" }, stream);
      string json = Encoding.UTF8.GetString(stream.ToArray());

      Assert.Contains("my_property", json);
      Assert.DoesNotContain("MyProperty", json);
    }

    /// <summary>
    /// Verify that deserialization correctly maps snake_case JSON to PascalCase properties
    /// when settings are created from an InMemoryRequestInvoker.
    /// </summary>
    [Fact]
    public void Create_WithConnection_DeserializesSnakeCaseProperties()
    {
      using var conn = new InMemoryConnection(Array.Empty<byte>(), 200);
      var client = new ElasticsearchClient(NciElasticsearchClientSettingsFactory.Create(conn));

      string json = "{\"my_property\":\"hello\"}";
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
      TestDocument result = client.SourceSerializer.Deserialize<TestDocument>(stream);

      Assert.Equal("hello", result.MyProperty);
    }

    /// <summary>
    /// Verify that the provided InMemoryRequestInvoker is wired up and used for requests —
    /// an error status code from the connection should produce a failed response.
    /// </summary>
    [Fact]
    public void Create_WithConnection_UsesProvidedConnection()
    {
      // Use a 503 connection so we can verify the status code flows through.
      using var conn = new InMemoryConnection(Array.Empty<byte>(), 503);
      var client = new ElasticsearchClient(NciElasticsearchClientSettingsFactory.Create(conn));

      var response = client.Search<TestDocument>(s => s.Indices("test-index"));

      Assert.Equal(503, response.ApiCallDetails.HttpStatusCode);
    }

    /// <summary>
    /// Simple document for testing serialization/deserialization.
    /// </summary>
    private class TestDocument
    {
      public string MyProperty { get; set; }
    }
  }
}
