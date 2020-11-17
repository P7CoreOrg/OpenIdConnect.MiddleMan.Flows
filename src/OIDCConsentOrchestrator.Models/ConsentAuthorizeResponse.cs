using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OIDCConsentOrchestrator.Models
{
    public class ConsentAuthorizeResponse: BaseResponse
    {
        [JsonPropertyName("authorized")]
        public bool Authorized { get; set; }
        [JsonPropertyName("scopes")] 
        public List<string> Scopes { get; set; }
        [JsonPropertyName("subject")]
        public string Subject { get; set; }
    }
}
