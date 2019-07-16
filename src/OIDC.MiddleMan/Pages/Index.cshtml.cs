using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using OIDC.ReferenceWebClient.Constants;

using OIDC.ReferenceWebClient.Data;
using OIDC.ReferenceWebClient.Extensions;
using OIDC.ReferenceWebClient.Models;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Endpoints.ResponseHandling;

namespace OIDC.ReferenceWebClient.Pages
{
    public class IndexModel : PageModel
    {
        private SignInManager<ApplicationUser> _signInManager;
        private IOIDCResponseGenerator _oidcResponseGenerator;
        private IOIDCPipelineStore _oidcPipelineStore;

        public IndexModel(SignInManager<ApplicationUser> signInManager,
            IOIDCResponseGenerator oidcResponseGenerator, IOIDCPipelineStore oidcPipelineStore)
        {
            _signInManager = signInManager;
            _oidcResponseGenerator = oidcResponseGenerator;
            _oidcPipelineStore = oidcPipelineStore;
        }
        public List<Claim> Claims { get; set; }
        public DownstreamAuthorizeResponse IdTokenResponse { get; private set; }

        public async Task OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                string nonce = HttpContext.GetOIDCPipeLineKey();
                IdTokenResponse = await _oidcPipelineStore.GetDownstreamIdTokenResponseAsync(nonce);

                Claims = Request.HttpContext.User.Claims.ToList();
            }
        }
        public async Task<IActionResult> OnPostWay2(string data)
        {
            string nonce = HttpContext.GetOIDCPipeLineKey();
            
            await _oidcPipelineStore.StoreDownstreamCustomDataAsync(nonce, new Dictionary<string, object> {
                { "prodInstance",Guid.NewGuid()}
            });
          
            var result = await _oidcResponseGenerator.CreateAuthorizeResponseActionResultAsync(nonce, true);
            await _signInManager.SignOutAsync();// we don't want our loggin hanging around
            return result;

        }
    }
}
