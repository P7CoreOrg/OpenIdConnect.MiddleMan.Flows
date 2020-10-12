using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WebClient.Constants;
using WebClient.Models;

namespace WebClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IMemoryCache cache, ILogger<IndexModel> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public List<Claim> Claims { get; set; }
        public OpenIdConnectSessionDetails OpenIdConnectSessionDetails { get; set; }
        public void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                var key = this.GetJsonCookie<string>(".oidc.memoryCacheKey");

                var oidcMessage = _cache.Get<OpenIdConnectMessage>(key);

                OpenIdConnectSessionDetails = HttpContext.Session.Get<OpenIdConnectSessionDetails>(Wellknown.OIDCSessionKey);

                Claims = Request.HttpContext.User.Claims.ToList();
            }
        }
    }
}
