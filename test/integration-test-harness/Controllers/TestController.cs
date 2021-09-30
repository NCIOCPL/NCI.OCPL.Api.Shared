using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Nest;


namespace integration_test_harness.Controllers
{
  /// <summary>
  /// Controller for using in tests.
  /// </summary>
  [Route("test")]
  public class TestController : ControllerBase
  {
    private readonly IElasticClient _elasticClient;

    private readonly ESIndexOptions _indexConfig;

    /// <summary>
    /// Constructor.
    /// <remarks>
    /// DO NOT DO THIS!!! The ES client and setup options don't belong in a controller.
    /// They're just here so we don't have to make the test any more complex than it
    /// already needs to be.
    /// </remarks>
    /// </summary>
    /// <param name="elasticClient">Elasticsearch client instance.</param>
    /// <param name="config">Configuration/settings for the query.</param>
    /// <returns></returns>
    public TestController(IElasticClient elasticClient, IOptions<ESIndexOptions> config)
      => (_elasticClient, _indexConfig) = (elasticClient, config.Value);

    /// <summary>
    /// Get an object which uses custom serialization.
    /// </summary>
    [HttpGet("custom-serialization/{identifier}")]
    public async Task<CustomSerializationModel> CustomSerialization(string identifier)
    {
      // Again, don't put Elasticsearch queries into the controller. This is only being
      // done in order to keep the test harness super-simple.
      IndexName index = Indices.Index(this._indexConfig.AliasName);

      IGetResponse<CustomSerializationModel> resp = null;

      try
      {
          IGetRequest  req = new GetRequest(index, identifier);
          resp = await _elasticClient.GetAsync<CustomSerializationModel>(req);
      }
      catch (System.Exception)
      {

          throw;
      }


      return resp.Source;
    }
  }
}