using OIDCConsentOrchestrator.Models;
using OIDCConsentOrchestrator.Models.Client;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.Services
{
    public interface IConsentExternalService
    {
        Task<ConsentAuthorizeResponse> PostAuthorizationRequestAsync(ConsentDiscoveryDocumentResponse discovery, ConsentAuthorizeRequest requestObject);
    }
}
