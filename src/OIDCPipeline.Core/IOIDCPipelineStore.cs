using OIDCPipeline.Core.Validation.Models;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IOIDCPipelineStore
    {
        Task StoreOriginalIdTokenRequestAsync(string key, ValidatedAuthorizeRequest request);
        Task<ValidatedAuthorizeRequest> GetOriginalIdTokenRequestAsync(string key);
        Task StoreDownstreamIdTokenResponseAsync(string key, IdTokenResponse response);
        Task<IdTokenResponse> GetDownstreamIdTokenResponseAsync(string key);
        Task DeleteStoredCacheAsync(string key);
    }
}
