using OIDCPipeline.Core.Validation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IOIDCPipelineStore
    {
        Task StoreOriginalIdTokenRequestAsync(string key, ValidatedAuthorizeRequest request);
        Task<ValidatedAuthorizeRequest> GetOriginalIdTokenRequestAsync(string key);
        Task StoreDownstreamIdTokenResponseAsync(string key, IdTokenResponse response);
        Task StoreDownstreamCustomDataAsync(string key, Dictionary<string,object> custom);
        Task<IdTokenResponse> GetDownstreamIdTokenResponseAsync(string key);
        Task DeleteStoredCacheAsync(string key);
    }
}
