using System;
using System.Text;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class InMemoryConnectionTests
  {
    /// <summary>
    /// For mock query.
    /// </summary>
    class TestType
    {
      public string Field1 { get; set; }
      public int Field2 { get; set; }
    }

    /// <summary>
    /// Verify that using InMemoryConnection doesn't result in an exception.
    /// (Checking for issues related to the "x-elastic-product" header.)
    /// </summary>
    [Fact]
    public void InMemoryConnection_NoExceptions()
    {
      var pool = new SingleNodePool(new Uri("http://localhost:9200"));

      using(InMemoryConnection conn = new InMemoryConnection(MockEmptyResponse, 200))
      {
        var connectionSettings = new ElasticsearchClientSettings(pool, conn);
        ElasticsearchClient client = new ElasticsearchClient(connectionSettings);

        var response = client.Search<TestType>(s => s
                  .Indices("AliasName")
                  .Query(q => q
                      .MatchAll()
                  )
              );
      }
    }

    /// <summary>
    /// Simulates a "no results found" response from Elasticsearch so we
    /// have something for tests where we don't care about the response.
    /// </summary>
    private byte[] MockEmptyResponse
    {
      get
      {
        string empty = @"
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
        ""total"": {
            ""value"": 0,
            ""relation"": ""eq""
        },
        ""max_score"": null,
        ""hits"": []
    }
}";
        byte[] byteArray = Encoding.UTF8.GetBytes(empty);
        return byteArray;
      }
    }
  }

}
