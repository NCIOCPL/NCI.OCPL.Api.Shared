using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Elastic.Transport;
using Elastic.Transport.Products.Elasticsearch;

#nullable enable

namespace NCI.OCPL.Api.Common.Testing
{
    /// <summary>
    /// A mock Elasticsearch connection, allowing inspection of values that would be passed
    /// to the Elasticsearch server as well as creation of simulated responses.
    /// </summary>
    /// <remarks>
    /// Use <see cref="RegisterRequestHandlerForType" /> to set simulated Elasticsearch responses.
    /// </remarks>
      public class ElasticsearchInterceptingConnection : IRequestInvoker
    {
        /// <summary>
        /// Container for simulated Elasticsearch responses.
        /// </summary>
        public class ResponseData : IDisposable
        {
            private bool disposed;

            /// <summary>
            /// Stream representing the response body.
            /// </summary>
            public Stream? Stream { get; set; }

            /// <summary>
            /// The simulated Elasticsearch HTTP status code.  Required if Stream is set.
            /// </summary>
            public int? StatusCode { get; set; }

            /// <summary>
            /// The simulated response MIME type.
            /// </summary>
            public string? ResponseMimeType { get; set; }

            /// <summary>
            /// For the IDispose pattern.
            /// </summary>
            protected virtual void Dispose(bool disposing)
            {
              if (!disposed)
              {
                if (disposing)
                {
                  Stream?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposed = true;
              }
            }

            /// <summary>
            /// For the IDispose pattern.
            /// </summary>
            public void Dispose()
            {
              // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
              Dispose(disposing: true);
              GC.SuppressFinalize(this);
            }
        }

        private Dictionary<Type, object> _callbackHandlers = new Dictionary<Type, object>();
        private Action<string, ResponseData>? _defCallbackHandler = null;

        /// <summary>
        /// Gets the response factory for creating responses
        /// </summary>
        public ResponseFactory ResponseFactory { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ElasticsearchInterceptingConnection()
        {
            ResponseFactory = new ElasticsearchResponseFactory();
        }

        /// <summary>
        /// For the IDispose pattern.
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Register a Request Handler for a given return type.
        /// NOTE: DO NOT REGISTER BOTH A CLASS AND ITS BASE CLASS!!!
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="callback"></param>
        public void RegisterRequestHandlerForType<TReturn>(Action<string, ResponseData> callback)
            where TReturn : class
        {
            Type returnType = typeof(TReturn);
            Type? handlerType = null;

            //Loop through the register handlers and see if our type is registered, OR
            //if a base class is registered.
            foreach (Type type in _callbackHandlers.Keys)
            {
                if (returnType == type || returnType.GetTypeInfo().IsSubclassOf(type))
                {
                    handlerType = type;
                    break;
                }
            }

            //If there is no handler, then add one.
            //If there is one, then error out
            if (handlerType == null)
            {
                _callbackHandlers.Add(typeof(TReturn), (object)callback);
            } else
            {
                throw new ArgumentException(
                    String.Format(
                        "There is already a handler defined that would be called for type. Trying to add for: {0}, Already Existing: {1}",
                        returnType.ToString(),
                        handlerType.ToString()
                    ));
            }
        }

        /// <summary>
        /// Allows for registration of a general request handler if a more specific one
        /// isn't registered.
        /// </summary>
        /// <param name="callback"></param>
        public void RegisterDefaultHandler(Action<string, ResponseData> callback)
        {
            if (_defCallbackHandler != null)
                throw new ArgumentException("Cannot add more than one default handler");

            this._defCallbackHandler = callback;
        }

        /// !!!! DO NOT OVERRIDE THIS METHOD! !!!!
        /// This is the shared guts of the Request/RequestAsync methods. It really ought to be private,
        /// but the ResponseBuilder class required of those methods is static (it not only can't be mocked,
        /// it can't even be passed in), rendering both methods untestable. Making this one protected at least
        /// allows the real logic to be tested.
        protected void ProcessRequest<TReturn>(Endpoint endpoint, ResponseData responseData)
            where TReturn : class
        {
            Type returnType = typeof(TReturn);
                bool foundHandler = false;

                // Loop through the registered handlers and see if our type is registered, OR
                // if a base class is registered.
                foreach (Type type in _callbackHandlers.Keys)
                {
                    if (returnType == type || returnType.GetTypeInfo().IsSubclassOf(type))
                    {
                        foundHandler = true;

                        Action<string, ResponseData> callback =
                            (Action<string, ResponseData>)_callbackHandlers[typeof(TReturn)];

                        callback(
                            endpoint.ToString(),
                            responseData
                        );

                    break;
                }
            }

            //If we did not find one, then fallback to the default.
            if (!foundHandler && _defCallbackHandler != null)
            {
                foundHandler = true;
                _defCallbackHandler(
                    endpoint.ToString(),
                    responseData
                );
            }

            //If we did not find any, throw an exception
            if (!_callbackHandlers.ContainsKey(typeof(TReturn)) && _defCallbackHandler == null)
                throw new ArgumentOutOfRangeException("There is no callback handler for defined for type, " + typeof(TReturn).ToString());

            // If the dev writing the test DID provide response data, but DID NOT set a MIME type, we'll just have to
            // assume they meant to set "applicaton/json" since that's what Elasticsearch normally sends back.
            if (String.IsNullOrWhiteSpace(responseData.ResponseMimeType)
                && responseData.StatusCode.HasValue
                && responseData.Stream != null)
            {
                responseData.ResponseMimeType = "application/json";
            }

            // This is much friendlier than the "Attempt to read a closed stream" message that will otherwise occur.
            if ( responseData.Stream != null && !responseData.StatusCode.HasValue)
            {
                throw new ArgumentException("If a response stream is set, a status code must also be set.");
            }

            //Basically all requests, even HEAD requests (e.g. AliasExists) need to have a stream to work correctly.
            //Note, a stream of nothing is still a stream.  So if you did not set a stream, we will do it for you.
            //I am sure this will cause issues when trying to test failures of other kinds...  Good use of 4hrs tracking
            //this stupid issue down.
            if (responseData.Stream == null)
            {
                responseData.Stream = new MemoryStream(new byte[0]);
            }
        }

        /// <summary>
        /// Synchronous request implementation
        /// </summary>
        public TResponse Request<TResponse>(Endpoint endpoint, BoundConfiguration boundConfiguration, PostData? postData, CancellationToken cancellationToken = default)
            where TResponse : TransportResponse, new()
        {
            using(ResponseData responseData = new ResponseData())
            {
              this.ProcessRequest<TResponse>(endpoint, responseData);

              // Ensure we have a stream (even if empty)
              var stream = responseData.Stream ?? new MemoryStream(new byte[0]);

              return ResponseFactory.Create<TResponse>(endpoint, boundConfiguration, postData, null,
                  responseData.StatusCode, null, stream,
                  responseData.ResponseMimeType ?? "application/json", 0, null, null);
            }
        }

        /// <summary>
        /// Asynchronous request implementation
        /// </summary>
        public async Task<TResponse> RequestAsync<TResponse>(Endpoint endpoint, BoundConfiguration boundConfiguration, PostData? postData, CancellationToken cancellationToken = default)
            where TResponse : TransportResponse, new()
        {
            using( ResponseData responseData = new ResponseData())
            {
              this.ProcessRequest<TResponse>(endpoint, responseData);

              // Ensure we have a stream (even if empty)
              var stream = responseData.Stream ?? new MemoryStream(new byte[0]);

              return await Task.FromResult(ResponseFactory.Create<TResponse>(endpoint, boundConfiguration, postData, null,
                  responseData.StatusCode, null, stream,
                  responseData.ResponseMimeType ?? "application/json", 0, null, null));
            }
        }        /// <summary>
        /// Helper to read stream to byte array
        /// </summary>
        private byte[] ReadStreamToBytes(Stream stream)
        {
            if (stream == null)
                return new byte[0];

            stream.Position = 0;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Helper function to extract the body of a request that would be sent to the Elasticsearch server.
        /// </summary>
        /// <param name="postData">The post data object</param>
        /// <returns>JsonDocument containing the request</returns>
        public JsonDocument? GetRequestPost(PostData? postData)
        {
            //Some requests can have this as null.  That is ok...
            if (postData == null)
                return null;

            String postBody = string.Empty;

            using (MemoryStream stream = new MemoryStream())
            {
                postData.Write(stream, new TransportConfiguration(), false);
                postBody = Encoding.UTF8.GetString(stream.ToArray());
            }

            return JsonDocument.Parse(postBody);
        }
    }
}
