using System;
using System.IO;
using System.Text;

using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class TestingToolsTests
  {
    [Fact]
    public void GetTestFileAsBytes_NonexistingFile()
    {
      Assert.Throws<FileNotFoundException>(
        () => TestingTools.GetTestFileAsBytes("NonExistingFile.json")
      );
    }

    [Fact]
    public void GetTestFileAsBytes_ReadExistingFile()
    {
      string data = @"
        {
          ""key"": ""value"",
          ""key2"": ""value2""
        }";
      byte[] expected = Encoding.UTF8.GetBytes(data);

      // Write the data to a temporary location.
      string path = Path.GetTempFileName();
      File.WriteAllText(path, data);

      // Get the data back.
      byte[] actual = TestingTools.GetTestFileAsBytes(path);

      Assert.Equal(expected, actual);
    }
  }
}