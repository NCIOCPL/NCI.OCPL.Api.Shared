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
    public void GetDataFileAsJson_Null()
    {
      Assert.Throws<ArgumentNullException>(
        () => TestingTools.GetDataFileAsJson(null)
      );
    }

    [Fact]
    public void GetDataFileAsJson_NonexistingFile()
    {
      Assert.Throws<FileNotFoundException>(
        () => TestingTools.GetDataFileAsJson("NonExistingFile.json")
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
    public void GetDataFileAsJson_SimpleString(string filename, string expectedValue)
    {
      JsonNode expected = JsonNode.Parse(expectedValue);

      string path = Path.Join("Tools/TestingTools/GetDataFileAsJson", filename);
      JsonNode actual = TestingTools.GetDataFileAsJson(path);

      Assert.True(JsonNode.DeepEquals(expected, actual), "JSON structures do not match.");
    }

  }
}