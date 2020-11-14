﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Common
{
    public class SeedSessionClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        private IHttpContextAccessor _httpContextAccessor;

        private string GuidN => Guid.NewGuid().ToString("N");
        public SeedSessionClaimsPrincipalFactory(
               IHttpContextAccessor httpContextAccessor,
               UserManager<IdentityUser> userManager,
               RoleManager<IdentityRole> roleManager,
               IOptions<IdentityOptions> optionsAccessor)
               : base(userManager, roleManager, optionsAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var sessionKey = GuidN;
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(".sessionKey", sessionKey));
            _httpContextAccessor.HttpContext.Session.SetString(sessionKey, sessionKey);
            return identity;
        }
    }
}
