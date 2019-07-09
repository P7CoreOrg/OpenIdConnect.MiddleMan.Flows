using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IOIDCPipelineStore
    {
        Task StoreOriginalIdTokenRequestAsync(IdTokenAuthorizationRequest request);
        Task<IdTokenAuthorizationRequest> GetOriginalIdTokenRequestAsync();
        Task StoreDownstreamIdTokenResponseAsync(IdTokenResponse response);
        Task<IdTokenResponse> GetDownstreamIdTokenResponseAsync();
        Task DeleteStoredCacheAsync();
    }
}
