using System;
using System.IO;
using System.Text;

using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class TestingToolsTests
  {
    [Fact]
    public void GetTestFileAsStream_Null()
    {
      Assert.Throws<ArgumentNullException>(
        () => TestingTools.GetTestFileAsStream(null)
      );
    }

    [Fact]
    public void GetTestFileAsStream_NonexistingFile()
    {
      Assert.Throws<FileNotFoundException>(
        () => TestingTools.GetTestFileAsStream("NonExistingFile.json")
      );
    }

    [Fact]
    public void GetTestFileAsStream_ReadExistingFile()
    {
      string data = @"
        {
          ""key"": ""value"",
          ""key2"": ""value2""
        }";
      byte[] expected = Encoding.UTF8.GetBytes(data);

      // Write the data to a temporary location.
      string path = Path.GetTempFileName();
      File.WriteAllBytes(path, expected);

      byte[] actual;
      int actualByteCount;
      using(Stream stream = TestingTools.GetTestFileAsStream(path))
      {
        // Allow a max number of bytes *much* longer than what's expected.
        byte[] buffer = new byte[expected.Length * 10];
        actualByteCount = stream.Read(buffer, 0, buffer.Length);
        // Copy only as many bytes as were actually read.
        actual = new byte[actualByteCount];
        Array.Copy(buffer, actual, actualByteCount);
      }

      Assert.Equal(expected.Length, actualByteCount);
      Assert.Equal(expected, actual);
    }
  }
}