using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IOIDCPipelineStore
    {
        Task StoreOriginalIdTokenRequestAsync(IdTokenAuthorizationRequest request);
        Task<IdTokenAuthorizationRequest> GetOriginalIdTokenRequestAsync();
        Task StoreDownstreamIdTokenResponse(IdTokenResponse response);
        Task<IdTokenResponse> GetDownstreamIdTokenResponse();
        Task DeleteStoredCacheAsync();
    }
}
