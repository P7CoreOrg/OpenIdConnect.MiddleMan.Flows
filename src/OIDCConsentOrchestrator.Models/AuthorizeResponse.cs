using System;
using System.Collections.Generic;

namespace OIDCConsentOrchestrator.Models
{
    public class AuthorizeResponse: BaseResponse
    {
        public bool Authorized { get; set; }
        public List<string> Scopes { get; set; }
        public string Subject { get; set; }
    }
}
