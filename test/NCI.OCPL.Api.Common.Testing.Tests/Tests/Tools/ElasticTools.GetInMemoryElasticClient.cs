using Elastic.Clients.Elasticsearch;
using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class ElasticToolsTest
  {
    [Fact]
    public void GetInMemoryElasticClient()
    {
      ElasticsearchClient client = ElasticTools.GetInMemoryElasticClient("test-response.json");
      var response = client.SearchTemplate<TestType>(sd => sd
                .Indices("AliasName")
                .Params(pd => pd
                    .Add("searchstring", "search_term")
                    .Add("my_size", 10)
                )
            );

      Assert.True(response.IsValidResponse);
      var totalHits = response.Hits.Total.Match(t => t.Value, l => l);
      Assert.Equal(222, totalHits);
      Assert.Equal(20, response.Hits.Hits.Count);
      Assert.All(response.Hits.Hits, hit => Assert.NotNull(hit.Source));
      Assert.All(response.Hits.Hits, hit => Assert.IsType<TestType>(hit.Source));
    }
  }
}