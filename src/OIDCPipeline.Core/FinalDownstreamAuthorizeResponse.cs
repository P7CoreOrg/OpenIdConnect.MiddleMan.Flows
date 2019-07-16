using System.Collections.Generic;
using System.Collections.Specialized;
using OIDCPipeline.Core.Validation.Models;

namespace OIDCPipeline.Core
{
    public class FinalDownstreamAuthorizeResponse
    {
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public string ExpiresAt { get; set; }
        public string LoginProvider { get; set; }
        public ValidatedAuthorizeRequest Request { get; set; }
        public Dictionary<string, object> Custom { get; internal set; } = new Dictionary<string, object>();
    }
}
