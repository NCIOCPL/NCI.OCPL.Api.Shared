using System.IO;
using System.Text.Json;

using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class ElastcsearchTestingToolsTest
  {
    [Fact]
    public void EmptyResponse()
    {
      JsonDocument expected = JsonDocument.Parse(ElastcsearchTestingTools.MockEmptyResponseString);
      JsonDocument actual = JsonDocument.Parse(new StreamReader(ElastcsearchTestingTools.MockEmptyResponse).ReadToEnd());

      Assert.Equivalent(expected, actual, strict: true);
    }
  }
}