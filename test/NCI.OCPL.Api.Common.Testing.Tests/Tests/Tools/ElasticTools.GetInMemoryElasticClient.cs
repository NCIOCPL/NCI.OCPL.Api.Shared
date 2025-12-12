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
                .Index("AliasName")
                .Params(pd => pd
                    .Add("searchstring", "search_term")
                    .Add("my_size", 10)
                )
            );

      Assert.True(response.IsValidResponse);
      Assert.Equal(222, response.Total);
      Assert.Equal(20, response.Documents.Count);
      Assert.All(response.Documents, doc => Assert.NotNull(doc));
      Assert.All(response.Documents, doc => Assert.IsType<TestType>(doc));
    }
  }
}