using System;
using System.IO;

using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class TestingToolsTests
  {
    [Fact]
    public void GetStringAsStream_Null()
    {
      Assert.Throws<ArgumentNullException>(
        () => TestingTools.GetStringAsStream(null)
      );
    }

    [Theory]
    [InlineData("", new byte[] { })] // Empty string.
    [InlineData("value", new byte[]{0x76, 0x61, 0x6c, 0x75, 0x65})]
    public void GetStringAsStream_ValidStrings(string data, byte[] expected)
    {
      byte[] actual;
      int actualByteCount;
      using (Stream stream = TestingTools.GetStringAsStream(data))
      {
        // Allow a max number of bytes *much* longer than what's expected.
        int bufSize = (expected.Length > 0) ? expected.Length * 10 : 10;
        byte[] buffer = new byte[bufSize];
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