using Microsoft.AspNetCore.Identity;
using OIDCPipeline.Core;
using System.Threading.Tasks;

namespace WebClient.Services
{
    public class DefaultSigninManager : ISigninManager
    {
        private SignInManager<IdentityUser> _signinManager;

        public DefaultSigninManager(SignInManager<IdentityUser> signinManager)
        {
            _signinManager = signinManager;
        }
        public Task SignOutAsync()
        {
            return _signinManager.SignOutAsync();
        }
    }
}
