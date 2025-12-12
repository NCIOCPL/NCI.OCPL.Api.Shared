using System;
using System.IO;
using System.Text.Json;

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
      JsonDocument expected = JsonDocument.Parse(expectedValue);

      string path = Path.Join("Tools/TestingTools/GetDataFileAsJObject", filename);
      JsonDocument actual = TestingTools.GetDataFileAsJsonDocument(path);

      Assert.Equivalent(expected, actual, strict: true);
    }

  }
}