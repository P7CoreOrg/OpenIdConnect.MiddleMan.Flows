using Common;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Common.Services
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
