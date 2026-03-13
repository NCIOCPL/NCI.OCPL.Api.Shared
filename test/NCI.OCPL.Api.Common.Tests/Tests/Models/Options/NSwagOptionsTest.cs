using System.Text.Json;

using Xunit;

namespace NCI.OCPL.Api.Common.Models.Options
{
  /// <summary>
  /// NSwag configuration tests
  /// </summary>
  public partial class NSwagOptionsTest
  {
    [Fact]
    public void Serialize()
    {
      NSwagOptions expected = JsonSerializer.Deserialize<NSwagOptions>(@"
{
  ""Title"": ""Swagger Doc Title"",
  ""Description"": ""API Description""
}");

      NSwagOptions options = new NSwagOptions()
      {
        Title = "Swagger Doc Title",
        Description = "API Description"
      };

      string actualText = JsonSerializer.Serialize(options);
      NSwagOptions actual = JsonSerializer.Deserialize<NSwagOptions>(actualText);

      Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void Deserialize()
    {
      string expectedTitle = "Swagger Doc Title";
      string expectedDescription = "API Description";

      string input = @"
{
  ""Title"": ""Swagger Doc Title"",
  ""Description"": ""API Description""
}";

      NSwagOptions actual = JsonSerializer.Deserialize<NSwagOptions>(input);

      Assert.Equal(expectedTitle, actual.Title);
      Assert.Equal(expectedDescription, actual.Description);
    }

  }
}