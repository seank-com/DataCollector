
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DataCollector
{
    public class ServiceHelper: IDisposable
    {
        private const string ServiceHost = <"http://mywebapp.azurewebsites.net/">;
        private const string StreamContentTypeHeader = "application/octet-stream";

        private bool _disposedValue = false; // To detect redundant calls
        private HttpClient _httpClient;

        public ServiceHelper()
        {
            _httpClient = new HttpClient();
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_httpClient != null)
                    {
                        _httpClient.Dispose();
                        _httpClient = null;
                    }
                }
                _disposedValue = true;
            }
        }

        public async Task<JObject> SendRequestAsync(string requestUrl, byte[] data)
        {
            JObject result = new JObject();
            try
            {
                MemoryStream memoryStream = new MemoryStream(data);

                if (string.IsNullOrWhiteSpace(requestUrl))
                {
                    requestUrl = string.Format("{0}", ServiceHost);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

                if (memoryStream != null)
                {

                    request.Content = new StreamContent(memoryStream);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(StreamContentTypeHeader);
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = null;
                    if (response.Content != null)
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                    }

                    if (!string.IsNullOrWhiteSpace(responseContent))
                    {
                        result = JObject.Parse(responseContent);
                    }
                }
                else
                {
                    result.Add("statusCode", response.StatusCode.ToString());
                    result.Add("reason", response.ReasonPhrase);
                }
            }
            catch (System.Exception e)
            {
                result.Add("Exception", e.Message);
            }

            return result;
        }
    }
}
