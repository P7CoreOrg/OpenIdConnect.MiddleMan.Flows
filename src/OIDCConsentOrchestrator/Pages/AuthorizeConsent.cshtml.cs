using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OIDCConsentOrchestrator.EntityFrameworkCore;
using OIDCConsentOrchestrator.EntityFrameworkCore.Services;
using OIDCConsentOrchestrator.Models;
using OIDCConsentOrchestrator.Models.Client;
using OIDCConsentOrchestrator.Services;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Validation.Models;

namespace OIDCConsentOrchestrator.Pages
{
    [Authorize]
    public class AuthorizeConsentModel : PageModel
    {
        private const string ScopeBaseUrl = "https://www.samplecompanyapis.com/auth/";

        private IOIDCPipeLineKey _oidcPipelineKey;
        private IOIDCPipelineStore _oidcPipelineStore;
        private ILogger<AuthorizeConsentModel> _logger;
        private IConsentExternalService _consentExternalService;
        private IConsentDiscoveryCacheAccessor _consentDiscoveryCacheAccessor;
        private IOIDCConsentOrchestratorAdmin _oidcConsentOrchestratorAdmin;

        public AuthorizeConsentModel(
            IConsentExternalService consentExternalService,
            IConsentDiscoveryCacheAccessor consentDiscoveryCacheAccessor,
            IOIDCConsentOrchestratorAdmin oidcConsentOrchestratorAdmin, 
            IOIDCPipeLineKey oidcPipelineKey,
            IOIDCPipelineStore oidcPipelineStore,
            ILogger<AuthorizeConsentModel> logger)
        {
            _consentExternalService = consentExternalService;
            _consentDiscoveryCacheAccessor = consentDiscoveryCacheAccessor;
            _oidcConsentOrchestratorAdmin = oidcConsentOrchestratorAdmin;
            _oidcPipelineKey = oidcPipelineKey;
            _oidcPipelineStore = oidcPipelineStore;
            _logger = logger;
        }

        public ValidatedAuthorizeRequest OriginalAuthorizationRequest { get; private set; }
        public List<ConsentResponseContainer> ConsentResponseContainers { get; private set; }
        public List<ExternalServiceEntity> ExternalServiceEntities { get; private set; }
        public string NameIdentifier { get; private set; }
        public string[] Scopes { get; private set; }

        public class ConsentResponseContainer
        {
            public ConsentDiscoveryDocumentResponse DiscoveryDocument { get; set; }
            public ConsentAuthorizeResponse Response { get; set; }
            public ConsentAuthorizeRequest Request { get; set; }
        }
        public async Task OnGetAsync()
        {
            NameIdentifier = User.Claims.GetClaimsByType(".externalNamedIdentitier").FirstOrDefault().Value;




            var key = _oidcPipelineKey.GetOIDCPipeLineKey();
            OriginalAuthorizationRequest = await _oidcPipelineStore.GetOriginalIdTokenRequestAsync(key);

            var queryScopes = (from item in OriginalAuthorizationRequest.Raw
                               where item.Key == "scope"
                               let scopes = item.Value.Split(" ")
                               from cItem in scopes
                               where cItem.StartsWith(ScopeBaseUrl)
                               select cItem).ToList();


            ConsentResponseContainers = new List<ConsentResponseContainer>();
            ExternalServiceEntities = await _oidcConsentOrchestratorAdmin.GetAllExternalServiceEntitiesAsync();
            foreach(var es in ExternalServiceEntities)
            {
                var queryScopesService = (from item in queryScopes
                                         where item.StartsWith($"{ScopeBaseUrl}{es.Name}")
                                         select item).ToList();
                if (queryScopesService.Any())
                {
                    var discoCache = _consentDiscoveryCacheAccessor.GetConsentDiscoveryCache(es);
                    var doco = await discoCache.GetAsync();

                    List<string> scopes = null;
                    switch (doco.AuthorizationType)
                    {
                        case Constants.AuthorizationTypes.Implicit:
                            scopes = null;
                            break;
                        case Constants.AuthorizationTypes.Subject:
                            scopes = null;
                            break;
                        case Constants.AuthorizationTypes.SubjectAndScopes:
                            scopes = queryScopes;
                            break;
                    }
                    if(doco.AuthorizationType != Constants.AuthorizationTypes.Implicit)
                    {
                        var request = new ConsentAuthorizeRequest
                        {
                            AuthorizeType = doco.AuthorizationType,
                            Scopes = scopes,
                            Subject = NameIdentifier
                        };
                        var response = await _consentExternalService.PostAuthorizationRequestAsync(doco, request);

                        var consentResponseContainer = new ConsentResponseContainer()
                        {
                            DiscoveryDocument = doco,
                            Request = request,
                            Response = response
                        };
                        ConsentResponseContainers.Add(consentResponseContainer);
                    }
                    else
                    {
                        ConsentResponseContainers.Add(new ConsentResponseContainer {
                            DiscoveryDocument = doco,
                            Request = null,
                            Response = null
                        });
                    }
                   
                }
            } 
        }
        
    }
}
