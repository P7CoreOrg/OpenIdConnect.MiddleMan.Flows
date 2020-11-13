using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Services
{
    public interface IOIDCConsentOrchestratorAdmin
    {
        Task<ExternalServiceEntity> UpsertEntityAsync(ExternalServiceEntity entity);
        Task<List<ExternalServiceEntity>> GetAllExternalServiceEntitiesAsync();
    }

}
