using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IClientSecretStore
    {
        Task<string> FetchClientSecretAsync(string scheme, string clientId);
        Task<List<string>> FetchAllowedProtocolParamatersAsync(string scheme);
    }
}
