using System;

using Elastic.Clients.Elasticsearch;

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
            string responseBody = TestingTools.ReadTestFile(testFile);

            var connectionSettings = TestingElasticsearchClientSettingsFactory.Create(responseBody, 200);

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
          var connectionSettings = TestingElasticsearchClientSettingsFactory.Create(String.Empty, statusCode);

          return new ElasticsearchClient(connectionSettings);
        }

  }
}