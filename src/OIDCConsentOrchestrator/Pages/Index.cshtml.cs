using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OIDCConsentOrchestrator.EntityFrameworkCore;
using OIDCConsentOrchestrator.EntityFrameworkCore.Services;
using OIDCConsentOrchestrator.Models.Client;
using OIDCConsentOrchestrator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.Pages
{
    public class IndexModel : PageModel
    {
        public class Container
        {
            public ConsentDiscoveryDocumentResponse ConsentDiscoveryDocumentResponse { get; set; }
            public ExternalServiceEntity ExternalServiceEntity { get; set; }
        }
        public List<Container> Containers { get; set; }
        private readonly IConsentDiscoveryCacheAccessor _consentDiscoveryCacheAccessor;
        private readonly IOIDCConsentOrchestratorAdmin _oidcConsentOrchestratorAdmin;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IConsentDiscoveryCacheAccessor consentDiscoveryCacheAccessor,
            IOIDCConsentOrchestratorAdmin oIDCConsentOrchestratorAdmin,  
            ILogger<IndexModel> logger)
        {
            _consentDiscoveryCacheAccessor = consentDiscoveryCacheAccessor;
            _oidcConsentOrchestratorAdmin = oIDCConsentOrchestratorAdmin;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Containers = new List<Container>();
            var externalServiceEntities = await _oidcConsentOrchestratorAdmin.GetAllExternalServiceEntitiesAsync();
            foreach (var es in externalServiceEntities)
            {
                
                var discoCache = _consentDiscoveryCacheAccessor.GetConsentDiscoveryCache(es);
                var doco = await discoCache.GetAsync();

                Containers.Add(new Container
                {
                    ExternalServiceEntity = es,
                    ConsentDiscoveryDocumentResponse = doco
                });

               
            }
        }
    }
}
