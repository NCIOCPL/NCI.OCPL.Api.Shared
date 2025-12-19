using System.IO;
using System.Text.Json.Nodes;

using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class ElastcsearchTestingToolsTest
  {
    /**
      * Test to ensure that the mock empty response matches the original string.
      */
    [Fact]
    public void EmptyResponse()
    {
      // Parse the string version of an empty response.
      JsonNode expected = JsonNode.Parse(ElastcsearchTestingTools.MockEmptyResponseString);

      // Parse what comes back as a stream.
      JsonNode actual = JsonNode.Parse(new StreamReader(ElastcsearchTestingTools.MockEmptyResponse).ReadToEnd());

      // Compare JSON structures - JsonNode.DeepEquals ignores property order.
      Assert.True(JsonNode.DeepEquals(expected, actual), "JSON structures do not match.");
    }
  }
}