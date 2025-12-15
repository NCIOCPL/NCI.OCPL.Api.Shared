using System;

using Xunit;

using NCI.OCPL.Api.Common.Testing;

namespace NCI.OCPL.Api.Common
{
  /// <summary>
  /// RegisterDefaultHandler tests
  /// </summary>
  public partial class ElasticsearchInterceptingConnectionTest
  {
    /// <summary>
    /// Fail to set up multiple default handlers.
    /// </summary>
    [Fact]
    public void RegisterRequestHandlerForType_MultipleDefaults()
    {
      void callback(RequestData req, object res) { }
      void callback2(RequestData req, object res) { }

      ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();

      // First registration should always succeed.
      conn.RegisterDefaultHandler(callback);

      Exception ex = Assert.Throws<ArgumentException>(
        () => conn.RegisterDefaultHandler(callback2)
      );

      Assert.Equal("Cannot add more than one default handler", ex.Message);
    }
  }
}