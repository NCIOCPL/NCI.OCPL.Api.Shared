using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Xunit;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class ElasticToolsTest
  {
    /// <summary>
    /// Test that various simulated Elasticsearch status codes result in the response being
    /// flagged as expected.
    /// </summary>
    /// <param name="returnCode">The simulated response status code.</param>
    /// <param name="expectedValid">Is the simulated response expected to be valid?</param>
    [Theory]
    [InlineData(200, true)]
    [InlineData(204, true)]
    [InlineData(400, false)]
    [InlineData(403, false)]
    [InlineData(404, false)]
    [InlineData(408, false)]
    async public void GetErrorElasticClient_InvalidResponse(int returnCode, bool expectedValid)
    {
      ElasticsearchClient client = ElasticTools.GetErrorElasticClient(returnCode);
      SearchRequest request = new SearchRequest("someIndex")
      {
        Query = new TermQuery { Field = "someField", Value = "someValue" }
      };

      SearchResponse<TestType> response = await client.SearchAsync<TestType>(request);

      Assert.Equal(expectedValid, response.IsValidResponse);
    }
  }
}