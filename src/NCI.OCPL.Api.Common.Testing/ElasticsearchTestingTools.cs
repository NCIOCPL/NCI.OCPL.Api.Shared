using System.IO;
using System.Text;

namespace NCI.OCPL.Api.Common.Testing
{
  /// <summary>
  /// Helper tools for loading responses from files and
  /// deserializing XML to objects.
  /// </summary>
  public static class ElasticsearchTestingTools
  {
    /// <summary>
    /// String for simulating an empty response from Elasticsearch.
    /// </summary>
    /// <value></value>
    static public string MockEmptyResponseString
    {
      get
      {
        return @"
{
    ""took"": 223,
    ""timed_out"": false,
    ""_shards"": {
        ""total"": 1,
        ""successful"": 1,
        ""skipped"": 0,
        ""failed"": 0
    },
    ""hits"": {
        ""total"": 0,
        ""max_score"": null,
        ""hits"": []
    }
}";
      }
    }

    /// <summary>
    /// A simulated empty Elasticsearch response.
    /// </summary>
    static public Stream MockEmptyResponseStream
    {
      get
      {
        byte[] byteArray = Encoding.UTF8.GetBytes(MockEmptyResponseString);
        return new MemoryStream(byteArray);
      }
    }

  }
}