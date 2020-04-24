using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OIDC.Orchestrator.Data;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Endpoints.ResponseHandling;

namespace OIDC.Orchestrator.Pages
{
    public class IndexModel : PageModel
    {
        private SignInManager<ApplicationUser> _signInManager;
        private IOIDCResponseGenerator _oidcResponseGenerator;
        private IOIDCPipelineStore _oidcPipelineStore;

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(SignInManager<ApplicationUser> signInManager,
            IOIDCResponseGenerator oidcResponseGenerator, IOIDCPipelineStore oidcPipelineStore,
            ILogger<IndexModel> logger)
        {
            _signInManager = signInManager;
            _oidcResponseGenerator = oidcResponseGenerator;
            _oidcPipelineStore = oidcPipelineStore;
            _logger = logger;
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
