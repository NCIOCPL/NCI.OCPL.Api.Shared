#nullable enable // This should be removed once we move to nullables globally.

using System;
using System.Collections.Generic;

using Elastic.Transport;

namespace NCI.OCPL.Api.Common.Testing
{
  /// <summary>
  /// Helper class to create an in-memory connection for testing.
  ///
  /// The Elasticsearch client will throw an exdception if the response doesn't include an
  /// `x-elastic-product` header with a value of `Elasticsearch`.  Rather than litter tests
  /// with a bunch of code to add this header, we create this subclass of InMemoryRequestInvoker
  /// as a thin wrapper to automatically add the header to all responses.
  ///
  /// Also note, InMemoryRequestInvoker claims to implement IDisposable, but doesn't actually
  /// have a `Dispose(bool disposing)` overload; presumably since it doesn't actually make a database
  /// connection. Our class has nothing to dispose, and so explicitly relies on the base class'
  /// implementation to satisfy the IDisposable contract.
  /// </summary>
  public class InMemoryConnection : InMemoryRequestInvoker, IRequestInvoker, IDisposable
  {

    private readonly Dictionary<string, IEnumerable<string>> Headers = new Dictionary<string, IEnumerable<string>>
        {
          { "x-elastic-product", new[] { "Elasticsearch" } }
        };

    /// <summary>
    /// Create an in-memory connection to a mock Elasticsearch instance.
    ///
    /// This is a convenience wrapper around InMemoryRequestInvoker to ensure that the necessary
    /// `x-elastic-product` header is present.  If you need to customize headers, just use
    /// InMemoryRequestInvoker directly.
    /// </summary>
    /// <param name="responseBody">The simulated response body as a byte array.</param>
    /// <param name="statusCode">The status code to return</param>
    /// <param name="exception">Simulated exception to report</param>
    /// <param name="contentType">Simulated content type of the response</param>
    public InMemoryConnection(byte[] responseBody, int statusCode = 200, Exception? exception = null, string contentType = "application/json")
      : base(responseBody, statusCode, exception, contentType, new Dictionary<string, IEnumerable<string>>
        {
          { "x-elastic-product", new[] { "Elasticsearch" } }
        })
    {
    }

    /// <summary>
    /// Default constructor. Deliberately hidden to force use of the other constructor.
    /// </summary>
    private InMemoryConnection()
    {
      throw new NotImplementedException("If you're using this constructor, you should just use InMemoryRequestInvoker directly.");
    }

  }
}