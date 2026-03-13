using System.IO;
using System.Text.Json.Nodes;

using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class ElasticsearchTestingToolsTest
  {
    /**
      * Test to ensure that the mock empty response matches the original string.
      */
    [Fact]
    public void EmptyResponse()
    {
      // Parse the string version of an empty response.
      JsonNode expected = JsonNode.Parse(ElasticsearchTestingTools.MockEmptyResponseString);

      // Parse what comes back as a stream.
      JsonNode actual;
      using (var reader = new StreamReader(ElasticsearchTestingTools.MockEmptyResponseStream))
      {
        actual = JsonNode.Parse(reader.ReadToEnd());
      }

      // Compare JSON structures - JsonNode.DeepEquals ignores property order.
      Assert.True(JsonNode.DeepEquals(expected, actual), "JSON structures do not match.");
    }
  }
}