using Microsoft.Extensions.Options;
using OIDCConsentOrchestrator.EntityFrameworkCore.Services;
using OIDCPipeline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.EntityFrameworkCore.Stores
{
    class EFOIDCPipelineClientStore : IOIDCPipelineClientStore
    {
        
        private IOIDCConsentOrchestratorAdmin _oidcConsentOrchestratorAdmin;

        public EFOIDCPipelineClientStore(
            IOIDCConsentOrchestratorAdmin oidcConsentOrchestratorAdmin)
        {
            _oidcConsentOrchestratorAdmin = oidcConsentOrchestratorAdmin;
        }
        public async Task<List<string>> FetchAllowedProtocolParamatersAsync(string scheme)
        {
            return Enumerable.Empty<string>().ToList();
           
        }

        public async Task<ClientRecord> FetchClientRecordAsync(string scheme, string clientId)
        {
            var config = await _oidcConsentOrchestratorAdmin.GetDownStreamOIDCConfigurationByNameAsync(scheme);
            if(config == null)
            {
                throw new Exception($"scheme:{scheme} not found");
            }
            var entity = (from item in config.OIDCClientConfigurations
                         where item.ClientId == clientId
                         select item).FirstOrDefault();
            if (entity == null)
            {
                throw new Exception($"clientId:{clientId} not found");
            }
            var redirectUris = (from item in entity.RedirectUris
                                select item.RedirectUri).ToList();
            var result = new ClientRecord
            {
                ClientId = entity.ClientId,
                Secret = entity.ClientSecret,
                RedirectUris = redirectUris
            };
            return result;
        }
    }
}
