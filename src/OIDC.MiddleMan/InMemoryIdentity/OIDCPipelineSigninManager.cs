using Microsoft.AspNetCore.Identity;
using OIDC.ReferenceWebClient.Data;
using OIDCPipeline.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OIDC.ReferenceWebClient.InMemoryIdentity
{
    public class OIDCPipelineSigninManager: ISigninManager
    {
        private SignInManager<ApplicationUser> _signInManager;

        public OIDCPipelineSigninManager(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public Task SignOutAsync()
        {
            return _signInManager.SignOutAsync();
        }
    }
}
