using System.Configuration;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IClientSecretStore
    {
        Task<string> FetchClientSecretAsync(string scheme, string clientId);
    }
}
