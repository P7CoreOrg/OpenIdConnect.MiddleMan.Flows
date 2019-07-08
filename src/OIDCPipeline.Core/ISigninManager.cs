using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface ISigninManager
    {
        Task SignOutAsync();
    }
}
