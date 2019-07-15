using System.Collections.Specialized;
using OIDCPipeline.Core.Validation.Models;

namespace OIDCPipeline.Core
{
    public class IdTokenResponse
    {
       
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public string ExpiresAt { get; set; }
        public string LoginProvider { get; set; }
        public string CodeChallenge { get; set; }
        public string CodeChallengeMethod { get; set; }
        public NameValueCollection Extras { get; set; } = new NameValueCollection();
        public ValidatedAuthorizeRequest Request { get; set; }
    }
}
