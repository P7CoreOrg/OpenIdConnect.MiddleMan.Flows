using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Extensions;
using FluffyBunny4.DotNetCore.Services;
using IdentityModel.FluffyBunny4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OIDCConsentOrchestrator.EntityFrameworkCore;
using OIDCConsentOrchestrator.EntityFrameworkCore.Services;
using OIDCConsentOrchestrator.Linq;
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
        private IFluffyBunnyTokenService _fluffyBunnyTokenService;
        private FluffyBunny4TokenServiceConfiguration _FluffyBunny4TokenServiceConfiguration;
        private ITokenServiceDiscoveryCache _tokenServiceDiscoveryCache;
        private ISerializer _serializer;
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
            IFluffyBunnyTokenService fluffyBunnyTokenService,
            IOptions<FluffyBunny4TokenServiceConfiguration> optionsFluffyBunny4TokenServiceConfiguration,
            ITokenServiceDiscoveryCache tokenServiceDiscoveryCache,
            ISerializer serializer,
            ILogger<AuthorizeConsentModel> logger)
        {
            _consentExternalService = consentExternalService;
            _consentDiscoveryCacheAccessor = consentDiscoveryCacheAccessor;
            _oidcConsentOrchestratorAdmin = oidcConsentOrchestratorAdmin;
            _oidcPipelineKey = oidcPipelineKey;
            _oidcPipelineStore = oidcPipelineStore;
            _fluffyBunnyTokenService = fluffyBunnyTokenService;
            _FluffyBunny4TokenServiceConfiguration = optionsFluffyBunny4TokenServiceConfiguration.Value;
            _tokenServiceDiscoveryCache = tokenServiceDiscoveryCache;
            _serializer = serializer;
            _logger = logger;
        }

        public ValidatedAuthorizeRequest OriginalAuthorizationRequest { get; private set; }
        public List<ConsentResponseContainer> ConsentResponseContainers { get; private set; }
        public List<ExternalServiceEntity> ExternalServiceEntities { get; private set; }
        public string NameIdentifier { get; private set; }
        public string[] Scopes { get; private set; }
        public ArbitraryTokenTokenRequestV2 ArbitraryTokenTokenRequestV2 { get; private set; }
        public string JsonArbitraryTokenTokenRequestV2 { get; private set; }

        public class CustomPayloadContainer
        {
            public string Name { get; set; }
            public object CustomPayload { get; set; }
        }
        public class ConsentResponseContainer
        {
            public ConsentDiscoveryDocumentResponse DiscoveryDocument { get; set; }
            public ConsentAuthorizeResponse Response { get; set; }
            public ConsentAuthorizeRequest Request { get; set; }
            public ExternalServiceEntity ExternalServiceEntity { get; internal set; }
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
                            ExternalServiceEntity = es,
                            DiscoveryDocument = doco,
                            Request = request,
                            Response = response
                        };
                        ConsentResponseContainers.Add(consentResponseContainer);
                    }
                    else
                    {
                        ConsentResponseContainers.Add(new ConsentResponseContainer {
                            ExternalServiceEntity = es,
                            DiscoveryDocument = doco,
                            Request = null,
                            Response = null
                        });
                    }
                   
                }
            }
            var finalScopes = (from item in ConsentResponseContainers
                        where item.Response.Authorized == true
                        from scope in item.Response.Scopes
                        select scope).ToList();

            var claims = (from item in ConsentResponseContainers
                          where item.Response.Authorized == true && item.Response.Claims != null
                          from claim in item.Response.Claims
                          let c = new ConsentAuthorizeClaim 
                          { 
                              Type = $"{item.ExternalServiceEntity.Name}.{claim.Type}", 
                              Value = claim.Value
                          }
                          select c)
                          .DistinctBy(p => new { p.Type, p.Value })
                          .ToList(); 
            

            var customs = (from item in ConsentResponseContainers
                          where item.Response.Authorized == true && item.Response.CustomPayload != null
                          let c = new CustomPayloadContainer 
                          { 
                              Name = $"{item.ExternalServiceEntity.Name}", 
                              CustomPayload = item.Response.CustomPayload 
                          }
                          select c).ToList();

            var docoTokenService = await _tokenServiceDiscoveryCache.GetAsync();
            ArbitraryTokenTokenRequestV2 = new ArbitraryTokenTokenRequestV2() {
                Address = docoTokenService.TokenEndpoint,
                ClientId = _FluffyBunny4TokenServiceConfiguration.ClientId,
                ClientSecret = _FluffyBunny4TokenServiceConfiguration.ClientSecret,
                Subject = NameIdentifier,
                Scope = new HashSet<string>(),
                ArbitraryClaims = new Dictionary<string, List<string>>(),
                ArbitraryAmrs = new List<string>(),
                ArbitraryAudiences = new List<string>(),
                CustomPayload = null
            };

            if (customs.Any())
            {
                Dictionary<string, object> customMap = new Dictionary<string, object>();
                foreach(var custom in customs)
                {
                    customMap[custom.Name] = custom.CustomPayload;
                }
                ArbitraryTokenTokenRequestV2.CustomPayload = customMap;
            }
            foreach(var item in finalScopes)
            {
                ArbitraryTokenTokenRequestV2.Scope.Add(item);
            }
          
            foreach(var claim in claims)
            {
                if (!ArbitraryTokenTokenRequestV2.ArbitraryClaims.ContainsKey(claim.Type))
                {
                    ArbitraryTokenTokenRequestV2.ArbitraryClaims[claim.Type] = new List<string>();
                }
                ArbitraryTokenTokenRequestV2.ArbitraryClaims[claim.Type].Add(claim.Value);
            }
            JsonArbitraryTokenTokenRequestV2 = _serializer.Serialize(ArbitraryTokenTokenRequestV2);

            var httpClient = new HttpClient();
            var tokenPayload = await _fluffyBunnyTokenService.RequestArbitraryTokenAsync(httpClient,ArbitraryTokenTokenRequestV2);
        }

    }
}
