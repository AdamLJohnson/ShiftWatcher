using System.Text.Json;

namespace ShiftWatcher.OrcicornMonitor.Lambda.Services
{
    public interface IHttpRequestFactory
    {
        IHttpWebRequest Create();
    }

    public interface IHttpWebRequest
    {
        Task<T> GetAsync<T>(string url);
    }

    public class HttpRequestFactory : IHttpRequestFactory
    {
        public IHttpWebRequest Create()
        {
            return new HttpWebRequest();
        }
    }

    public class HttpWebRequest : IHttpWebRequest
    {
        private readonly HttpClient _client;

        public HttpWebRequest()
        {
            _client = new HttpClient();
        }

        public async Task<T> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            if (result == null)
                throw new Exception("Serialization error");
            return result;
        }
    }
}