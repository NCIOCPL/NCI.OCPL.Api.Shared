using System.Text.Json;

using Xunit;

namespace NCI.OCPL.Api.Common.Models.Options
{
  /// <summary>
  /// Elasticsearch configuration tests
  /// </summary>
  public partial class ElasticsearchOptionsTest
  {
    [Fact]
    public void Serialize()
    {
      ElasticsearchOptions expected = JsonSerializer.Deserialize<ElasticsearchOptions>(@"
{
  ""Servers"": ""Server list"",
  ""Userid"": ""the user id"",
  ""Password"": ""secret password"",
  ""MaximumRetries"": 3
}");

      ElasticsearchOptions options = new ElasticsearchOptions()
      {
        Servers = "Server list",
        Userid = "the user id",
        Password = "secret password",
        MaximumRetries = 3
      };

      string actualText = JsonSerializer.Serialize(options);
      ElasticsearchOptions actual = JsonSerializer.Deserialize<ElasticsearchOptions>(actualText);

      Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void Deserialize()
    {
      string expectedServers = "Server list";
      string expectedUserid = "the user id";
      string expectedPassword = "secret password";
      int expectedMaximumRetries = 5;

      string input = @"
{
  ""Servers"": ""Server list"",
  ""Userid"": ""the user id"",
  ""Password"": ""secret password"",
  ""MaximumRetries"": 5
}";

      ElasticsearchOptions actual = JsonSerializer.Deserialize<ElasticsearchOptions>(input);

      Assert.Equal(expectedServers, actual.Servers);
      Assert.Equal(expectedUserid, actual.Userid);
      Assert.Equal(expectedPassword, actual.Password);
      Assert.Equal(expectedMaximumRetries, actual.MaximumRetries);
    }
  }
}