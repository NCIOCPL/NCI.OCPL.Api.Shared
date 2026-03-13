using System;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace NCI.OCPL.Api.Common.Testing
{

    /// <summary>
    /// Tools for mocking elasticsearch clients
    /// </summary>
    public static class ElasticTools {

        /// <summary>
        /// Gets an ElasticsearchClient backed by an InMemoryConnection.  This is used to mock the
        /// JSON returned by the elastic search so that we test the Elasticsearch mappings to our models.
        /// </summary>
        /// <param name="testFile"></param>
        /// <returns></returns>
        public static ElasticsearchClient GetInMemoryElasticClient(string testFile) {

            //Get Response JSON
            byte[] responseBody = TestingTools.GetTestFileAsBytes(testFile);

            //While this has a URI, it does not matter, an InMemoryConnection never requests
            //from the server.
            var pool = new SingleNodePool(new Uri("http://localhost:9200"));

            // Setup ElasticSearch stuff using the contents of the JSON file as the client response.
            InMemoryConnection conn = new InMemoryConnection(responseBody);

            var connectionSettings = new ElasticsearchClientSettings(pool, conn);

            return new ElasticsearchClient(connectionSettings);
        }

        /// <summary>
        /// Gets an ElasticsearchClient which simulates a failed request.  Success is defined by
        /// statuses with a 200-series response, so anything from the 400 or 503 series
        /// should be treated as an error.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static ElasticsearchClient GetErrorElasticClient(int statusCode)
        {
          //While this has a URI, it does not matter, an InMemoryConnection never requests
          //from the server.
          var pool = new SingleNodePool(new Uri("http://localhost:9200"));

          //Get Response JSON
          byte[] responseBody = Array.Empty<byte>();

          InMemoryConnection conn = new InMemoryConnection(responseBody, statusCode: statusCode);

          var connectionSettings = new ElasticsearchClientSettings(pool, conn);

          return new ElasticsearchClient(connectionSettings);
        }

  }
}