using System.Collections.Generic;

namespace OIDCConsentOrchestrator.Models
{
    public class AuthorizeRequest
    {
        public AuthorizeTypes AuthorizeType { get; set; }
        public string Subject { get; set; }
        public List<string> Scopes { get; set; }
    }
}
