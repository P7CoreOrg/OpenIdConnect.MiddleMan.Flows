using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Services
{
    public interface IOIDCConsentOrchestratorAdmin
    {
        #region ExternalServices

        Task<ExternalServiceEntity> UpsertEntityAsync(ExternalServiceEntity entity);

        Task<List<ExternalServiceEntity>> GetAllExternalServiceEntitiesAsync();

        #endregion

        #region DownstreamOIDCConfiguration
        
        Task<RedirectUriEntity> UpsertEntityAsync(
            string downstreamOIDCConfigurationId,
            string oidcClientConfigurationId, 
            RedirectUriEntity entity);

        Task DeleteEntityAsync(RedirectUriEntity entity);

        Task<OIDCClientConfigurationEntity> UpsertEntityAsync(
            string downstreamOIDCConfigurationId, 
            OIDCClientConfigurationEntity entity);

        Task DeleteEntityAsync(OIDCClientConfigurationEntity entity);

        Task<DownstreamOIDCConfigurationEntity> UpsertEntityAsync(DownstreamOIDCConfigurationEntity entity);

        Task DeleteEntityAsync(DownstreamOIDCConfigurationEntity entity);

        Task<List<DownstreamOIDCConfigurationEntity>> GetAllDownstreamOIDCConfigurationEntitiesAsync();
        Task<DownstreamOIDCConfigurationEntity> GetDownStreamOIDCConfigurationByNameAsync(string name);
        #endregion
    }

}
