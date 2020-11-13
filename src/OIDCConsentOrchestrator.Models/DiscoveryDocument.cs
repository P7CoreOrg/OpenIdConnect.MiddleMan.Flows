using System.Collections.Generic;

namespace OIDCConsentOrchestrator.Models
{
    public class DiscoveryDocument
    {
        public string AuthorizeEndpoint { get; set; }
        public List<string> Scopes { get; set; }
        public AuthorizeTypes AuthorizeType { get; set; }
    }
}
