using OIDCConsentOrchestrator.Models;
using OIDCConsentOrchestrator.Models.Client;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.Services
{
    public class ConsentExternalService : IConsentExternalService
    {
        public ConsentExternalService()
        {

        }
        private static async Task<HttpResponseMessage> PostJsonContentAsync<T>(string uri, HttpClient httpClient, T obj)
        {

            var postRequest = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(obj)
            };

            var postResponse = await httpClient.SendAsync(postRequest);

            postResponse.EnsureSuccessStatusCode();

            return postResponse;
        }

        public async Task<ConsentAuthorizeResponse> PostAuthorizationRequestAsync(
            ConsentDiscoveryDocumentResponse discovery,
            ConsentAuthorizeRequest requestObject)
        {
            var httpClient = new HttpClient();
            using var httpResponse = await PostJsonContentAsync(discovery.AuthorizeEndpoint, httpClient, requestObject);
            if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
            {
                var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                return await System.Text.Json.JsonSerializer.DeserializeAsync<ConsentAuthorizeResponse>(contentStream, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true });
                 
            }
            throw new Exception("HTTP Response was invalid and cannot be deserialised.");
        }
    }
}
