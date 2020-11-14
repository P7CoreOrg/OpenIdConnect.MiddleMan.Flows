using System.Collections.Generic;

namespace OIDCConsentOrchestrator.Models
{
    public class AuthorizeRequest
    {
        public string AuthorizeType { get; set; }
        public string Subject { get; set; }
        public List<string> Scopes { get; set; }
    }
}
