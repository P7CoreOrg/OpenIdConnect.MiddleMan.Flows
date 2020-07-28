using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OIDC.Orchestrator.Data;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Endpoints.ResponseHandling;

namespace OIDC.Orchestrator.Pages
{
    class SomeObject
    {
        public string Name { get; set; }
    }
    class Custom
    {
        public string Name { get; set; }
        public List<int> Numbers { get; set; }
        public List<string> Strings { get; set; }
        public List<SomeObject> SomeObjects { get; set; }
        public SomeObject SomeObject { get; set; }


    }
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

            var custom = new Custom
            {
                Name = "Bugs Bunny",
                Numbers = new List<int>() { 1, 2, 3 },
                Strings = new List<string>() { "a", "bb", "ccc" },
                SomeObject = new SomeObject { Name = "Daffy Duck" },
                SomeObjects = new List<SomeObject>()
                {
                    new SomeObject { Name = "Daisy Duck"},
                    new SomeObject { Name = "Porky Pig"},
                }
            };
            var json = JsonConvert.SerializeObject(custom);

            await _oidcPipelineStore.StoreDownstreamCustomDataAsync(nonce, new Dictionary<string, object> {
                { "prodInstance",Guid.NewGuid()},
                { "extraStuff",custom}
            });

            var result = await _oidcResponseGenerator.CreateAuthorizeResponseActionResultAsync(nonce, true);
            await _signInManager.SignOutAsync();// we don't want our loggin hanging around
            return result;

        }
    }
}
