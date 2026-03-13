using System;
using System.IO;
using System.Text;

using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class TestingToolsTests
  {
    [Fact]
    public void GetPathToTestFile_Null()
    {
      Assert.Throws<ArgumentNullException>(
        () => TestingTools.GetPathToTestFile(null)
      );
    }

    [Theory]
    [InlineData("")]
    [InlineData("foo")]
    [InlineData("foo/bar")]
    [InlineData("foo/bar/baz")]
    public void GetPathToTestFile_Filename(string filename)
    {
      string basePath = Path.GetDirectoryName(this.GetType().Assembly.Location);
      string expected = Path.Join(basePath, "TestData", filename);

      string actual = TestingTools.GetPathToTestFile(filename);

      Assert.Equal(expected, actual);
    }

  }
}