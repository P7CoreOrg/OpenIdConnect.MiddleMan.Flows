using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IdentityModel;
using OIDCPipeline.Core;
using OIDCPipeline.Core.AuthorizationEndpoint;
using System;
using OIDC.ReferenceWebClient.Extensions;
using Microsoft.AspNetCore.Identity;
using OIDC.ReferenceWebClient.Data;
using OIDC.ReferenceWebClient.Discovery;

namespace OIDC.ReferenceWebClient.Controllers
{

    [ApiController]
    [Route("")]
    public class OIDCController : ControllerBase
    {
        private IGoogleDiscoveryCache _googleDiscoveryCache;
        private SignInManager<ApplicationUser> _signInManager;
        private ILogger<OIDCController> _logger;
        private IOIDCPipelineStore _oidcPipelineStore;
        private IAuthorizeRequestValidator _authorizeRequestValidator;

        public OIDCController(
            IGoogleDiscoveryCache googleDiscoveryCache,
            SignInManager<ApplicationUser> signInManager,
            IOIDCPipelineStore oidcPipelineStore,
            IAuthorizeRequestValidator authorizeRequestValidator,
            ILogger<OIDCController> logger)
        {
            _googleDiscoveryCache = googleDiscoveryCache;
            _signInManager = signInManager;
            _logger = logger;
            _oidcPipelineStore = oidcPipelineStore;
            _authorizeRequestValidator = authorizeRequestValidator;
        }
        // GET: api/OIDC
        [HttpGet]
        [Route(".well-known/openid-configuration")]
        public async Task<Dictionary<string, object>> GetWellknownOpenIdConfiguration()
        {
            var response = await _googleDiscoveryCache.GetAsync();
            var googleStuff = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Raw);
            googleStuff["authorization_endpoint"]
               = "https://localhost:5001/connect/authorize";

            return googleStuff;
        }

        // GET: api/OIDC/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/OIDC
        [HttpPost]
        [HttpGet]
        [Route("connect/authorize")]
        public async Task<IActionResult> PostConnectAuthorizeAsync()
        {
            /*
             https://localhost:44305/connect/authorize?
client_id=mvc
&redirect_uri=https%3A%2F%2Fp7core.127.0.0.1.xip.io%3A44311%2Fsignin-oidc
&response_type=id_token
&scope=openid%20profile
&response_mode=form_post
&nonce=636973229335838266.ZWJhM2U4M2YtYWNiYi00YjZkLTkwMWYtNjRmMjM3MWRiYTk5OWNkNDIzMWUtZmY4OS00YWE0LTk4MGUtMTdiMjYxNmNmZjRk&state=CfDJ8KOz5LEySMhBtqpccMk4UVhA1PvGQQvpqQBUyR-97TDZvaPuNquTLJIUxKMYzF-Ov_HHCnnmcTForzd5RJ4jmLONvcZLY3XCHnrhh9Sc2oR2Lv2HACvPVBMy2oYmmPBtNIoXroQ9WePE_KtPyFw8ntRsHIYMmT5a0fLKGeJcwK3ewoiRHxjKpOr9hXZau9f7CVVqMvtWC2ngWrFsEeh8S0YtRZQFT-7XyjE9dNiyKp_Z-4iBUbbqzVnT4GmEmErZXUjmBhmVsMLz5h9y_F3usRT3lg7LxUNamnJuROnYIqmJzf0fYVJq1mcB5hcUipo2SNcILG3xkUikc84VznSGvD7V_qFjOHVPtOEX02JH9M4ymb3iZtZSE9dDr2RkwTU7StoKgM-x195bBULpwms8weJO-kx5I6UrY_lmWl0SFqYN
&x-client-SKU=ID_NETSTANDARD2_0
&x-client-ver=5.4.0.0

             */
            NameValueCollection values;
            if (HttpMethods.IsGet(Request.Method))
            {
                values = Request.Query.AsNameValueCollection();
            }
            else if (HttpMethods.IsPost(Request.Method))
            {
                if (!Request.HasFormContentType)
                {
                    return new StatusCodeResult((int)HttpStatusCode.UnsupportedMediaType);
                }

                values = Request.Form.AsNameValueCollection();
            }
            else
            {
                return new StatusCodeResult((int)HttpStatusCode.MethodNotAllowed);
            }

            var result = await ProcessAuthorizeRequestAsync(values);
            var idTokenAuthorizationRequest = new IdTokenAuthorizationRequest
            {
                client_id = values.Get(OidcConstants.AuthorizeRequest.ClientId),
                client_secret = values.Get("client_secret"),
                nonce = values.Get(OidcConstants.AuthorizeRequest.Nonce),
                response_mode = values.Get(OidcConstants.AuthorizeRequest.ResponseMode),
                redirect_uri = values.Get(OidcConstants.AuthorizeRequest.RedirectUri),
                response_type = values.Get(OidcConstants.AuthorizeRequest.ResponseType),
                state = values.Get(OidcConstants.AuthorizeRequest.State),
                scope = values.Get(OidcConstants.AuthorizeRequest.Scope)
            };

            await _oidcPipelineStore.StoreOriginalIdTokenRequestAsync(HttpContext.Session.GetSessionId(), idTokenAuthorizationRequest);
            if (User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
            }

            return Redirect("~/");

            //    return result;
        }
        internal async Task<IActionResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters)
        {
            var result = await _authorizeRequestValidator.ValidateAsync(parameters);
            if (result.IsError)
            {
                return BadRequest(result.ErrorDescription);
            }
            return Ok();
        }
        // PUT: api/OIDC/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
