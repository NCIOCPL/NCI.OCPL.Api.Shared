using Elastic.Clients.Elasticsearch.Mapping;

namespace NCI.OCPL.Api.Common.Testing
{
  public partial class ElasticToolsTest
  {
    /// <summary>
    /// Mock business object.
    /// </summary>
    public class TestType
    {
      /// <summary>
      /// The Backend ID for this item
      /// </summary>
      /// <returns></returns>
      [Text(Name = "term")]
      public string Term { get; set; }
    }
  }
}
