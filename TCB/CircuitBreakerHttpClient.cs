using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TCB
{
    public class CircuitBreakerHttpClient : HttpClient
    {

        private const HttpCompletionOption defaultCompletionOption = HttpCompletionOption.ResponseContentRead;
        private CircuitBreaker CircuitBreaker { get; set; }

        // https://github.com/dotnet/corefx/blob/master/src/System.Net.Http/src/System/Net/Http/HttpClient.cs
        public CircuitBreakerHttpClient(CircuitBreaker circuitBreaker) : base()
        {
            CircuitBreaker = circuitBreaker;
        }

        public CircuitBreakerHttpClient(HttpMessageHandler handler) : base(handler)
        {
            CircuitBreaker = new CircuitBreaker();
        }

        public CircuitBreakerHttpClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {
            CircuitBreaker = new CircuitBreaker();
        }

        public new async Task<Stream> GetStreamAsync(string uri)
        {
            return await GetStreamAsync(new Uri(uri));
        }

        public new async Task<Stream> GetStreamAsync(Uri uri)
        {
            if (CircuitBreaker.AllowCall())
            {
                CircuitBreaker.CallStart();

                try
                {
                    var responseMessage = await base.GetStreamAsync(uri);
                    CircuitBreaker.RegisterSuccess();
                    CircuitBreaker.CallStop();

                    return responseMessage;
                }
                catch (Exception)
                {
                    CircuitBreaker.RegisterFailure();
                    CircuitBreaker.CallStop();
                    throw;
                }

            }

            throw new Exception("Open circuit exception");
        }

        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await SendAsync(request, defaultCompletionOption, CancellationToken.None);
        }

        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            return await SendAsync(request, completionOption, CancellationToken.None);
        }

        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await SendAsync(request, defaultCompletionOption, cancellationToken);
        }


        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (CircuitBreaker.AllowCall())
            {
                CircuitBreaker.CallStart();

                try
                {
                    var responseMessage = await base.SendAsync(request, completionOption, cancellationToken);

                    if (responseMessage.IsSuccessStatusCode)
                        CircuitBreaker.RegisterSuccess();
                    else
                        CircuitBreaker.RegisterFailure();

                    CircuitBreaker.CallStop();
                    return responseMessage;
                }
                catch (Exception e)
                {
                    CircuitBreaker.RegisterFailure();
                    CircuitBreaker.CallStop();
                    throw;
                }
            }

            var badResponseResponseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                ReasonPhrase = "Conflict. Open Circuit."
            };

            return badResponseResponseMessage;
        }


        public new Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return GetAsync(CreateUri(requestUri));
        }

        public new Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            return GetAsync(requestUri, defaultCompletionOption);
        }

        public new Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption)
        {
            return GetAsync(CreateUri(requestUri), completionOption);
        }

        public new Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption)
        {
            return GetAsync(requestUri, completionOption, CancellationToken.None);
        }

        public new Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken)
        {
            return GetAsync(CreateUri(requestUri), cancellationToken);
        }

        public new Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            return GetAsync(requestUri, defaultCompletionOption, cancellationToken);
        }

        public new Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            return GetAsync(CreateUri(requestUri), completionOption, cancellationToken);
        }

        public new Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            return SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), completionOption, cancellationToken);
        }

        public new Task<string> GetStringAsync(string requestUri)
        {
            return GetStringAsync(new Uri(requestUri));
        }

        public new async Task<string> GetStringAsync(Uri uri)
        {
            if (CircuitBreaker.AllowCall())
            {
                try
                {
                    CircuitBreaker.CallStart();
                    var result = await base.GetStringAsync(uri);

                    CircuitBreaker.RegisterSuccess();
                    CircuitBreaker.CallStop();
                    return result;
                }
                catch (Exception e)
                {
                    CircuitBreaker.RegisterFailure();
                    CircuitBreaker.CallStop();
                    throw;
                }
            }

            throw new Exception("Circuit Open Exception");
        }

        public new Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return DeleteAsync(CreateUri(requestUri));
        }

        public new Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
        {
            return DeleteAsync(requestUri, CancellationToken.None);
        }

        public new Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken)
        {
            return DeleteAsync(CreateUri(requestUri), cancellationToken);
        }

        public new Task<HttpResponseMessage> DeleteAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            return SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);
        }

        public new Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return PostAsync(CreateUri(requestUri), content);
        }

        public new Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        {
            return PostAsync(requestUri, content, CancellationToken.None);
        }

        public new Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            return PostAsync(CreateUri(requestUri), content, cancellationToken);
        }

        public new Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = content;
            return SendAsync(request, cancellationToken);
        }

        public new Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            return PutAsync(CreateUri(requestUri), content);
        }

        public new Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        {
            return PutAsync(requestUri, content, CancellationToken.None);
        }

        public new Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            return PutAsync(CreateUri(requestUri), content, cancellationToken);
        }

        public new Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = content;
            return SendAsync(request, cancellationToken);
        }

        public new async Task<byte[]> GetByteArrayAsync(string requestUri)
        {
            return await GetByteArrayAsync(CreateUri(requestUri));
        }
            

        public new async Task<byte[]> GetByteArrayAsync(Uri requestUri)
        {
            if (CircuitBreaker.AllowCall())
            {
                try
                {
                    CircuitBreaker.CallStart();
                    var result = await base.GetByteArrayAsync(requestUri);

                    CircuitBreaker.RegisterSuccess();
                    CircuitBreaker.CallStop();
                    return result;
                }
                catch (Exception e)
                {
                    CircuitBreaker.RegisterFailure();
                    CircuitBreaker.CallStop();
                    throw;
                }
            }

            throw new Exception("Circuit Open Exception");
        }

        public new void CancelPendingRequests()
        {
            CircuitBreaker.CancelPendingRequests();
            base.CancelPendingRequests();
        }

        private Uri CreateUri(String uri)
        {
            
            if (string.IsNullOrEmpty(uri))
            {
                return null;
            }
            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }

    }
}