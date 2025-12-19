using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class TestingToolsTests
  {
    [Fact]
    public void GetDataFileAsJObject_Null()
    {
      Assert.Throws<ArgumentNullException>(
        () => TestingTools.GetDataFileAsJsonDocument(null)
      );
    }

    [Fact]
    public void GetDataFileAsJObject_NonexistingFile()
    {
      Assert.Throws<FileNotFoundException>(
        () => TestingTools.GetDataFileAsJsonDocument("NonExistingFile.json")
      );
    }

    [Theory]
    [InlineData("structured.json", @"{
                                        ""string"": ""string-value"",
                                        ""integer"": 5,
                                        ""null"": null,
                                        ""object"": {
                                          ""member1"": ""member"",
                                          ""member2"": 42
                                        }
                                      }")]
    public void GetDataFileAsJObject_SimpleString(string filename, string expectedValue)
    {
      JsonNode expected = JsonNode.Parse(expectedValue);

      string path = Path.Join("Tools/TestingTools/GetDataFileAsJObject", filename);
      JsonDocument actual = TestingTools.GetDataFileAsJsonDocument(path);

      // We need to convert JsonDocument to JsonNode to do a deep comparison.
      JsonNode actualNode = JsonNode.Parse(actual.RootElement.GetRawText());

      Assert.True(JsonNode.DeepEquals(expected, actualNode), "JSON structures do not match.");
    }

  }
}