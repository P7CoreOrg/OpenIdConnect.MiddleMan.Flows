using Microsoft.Extensions.Logging;
using OIDCConsentOrchestrator.Models;
using OIDCConsentOrchestrator.Models.Client;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.Services
{
    public class ConsentExternalService : IConsentExternalService
    {
        private ILogger<ConsentExternalService> _logger;

        public ConsentExternalService(ILogger<ConsentExternalService> logger)
        {
            _logger = logger;
        }
        private static async Task<HttpResponseMessage> PostJsonContentAsync<T>(string uri, HttpClient httpClient, T obj)
        {

            var postRequest = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(obj)
            };

            var postResponse = await httpClient.SendAsync(postRequest);

           // postResponse.EnsureSuccessStatusCode();

            return postResponse;
        }

        public async Task<ConsentAuthorizeResponse> PostAuthorizationRequestAsync(
            ConsentDiscoveryDocumentResponse discovery,
            ConsentAuthorizeRequest requestObject)
        {
            try
            {
                var httpClient = new HttpClient();
                using var httpResponse = await PostJsonContentAsync(discovery.AuthorizeEndpoint, httpClient, requestObject);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var result = new ConsentAuthorizeResponse()
                    {
                        Subject = requestObject.Subject,
                        Scopes = requestObject.Scopes,
                        Authorized = false,
                        Error = new Error
                        {
                            Message = $"StatusCode={httpResponse.StatusCode}",
                            StatusCode = (int)httpResponse.StatusCode
                        }
                    };
                    if (httpResponse.Content is object)
                    {
                        var contentText = await httpResponse.Content.ReadAsStringAsync();
                        result.Error.Message = contentText;
                    }
                    _logger.LogError($"statusCode={httpResponse.StatusCode},content=\'{result.Error.Message}\'");
                    return result;
                }

 
                if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                    var consentAuthorizeResponse = await System.Text.Json.JsonSerializer.DeserializeAsync<ConsentAuthorizeResponse>(contentStream, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true });
                    return consentAuthorizeResponse;
                }
                throw new Exception("HTTP Response was invalid and cannot be deserialised.");

            }
            catch (Exception ex)
            {
                var result = new ConsentAuthorizeResponse()
                {
                    Subject = requestObject.Subject,
                    Scopes = requestObject.Scopes,
                    Authorized = false,
                    Error = new Error
                    {
                        Message = ex.Message,
                        StatusCode =  (int)HttpStatusCode.BadRequest
                    }
                };
                return result;
            }
        }
    }
}
