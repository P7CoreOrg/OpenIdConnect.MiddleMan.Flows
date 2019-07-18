using System.Net.Http;

namespace IdentityModelExtras
{
    public class DefaultHttpClientFactory : IDefaultHttpClientFactory
    {
        public HttpMessageHandler HttpMessageHandler { get; set; }
        public HttpClient HttpClient { get { return new HttpClient(); } }
    }
}