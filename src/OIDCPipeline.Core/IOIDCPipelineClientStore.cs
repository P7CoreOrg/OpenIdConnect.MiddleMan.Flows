using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IOIDCPipelineClientStore
    {
        Task<string> FetchClientSecretAsync(string scheme, string clientId);
        Task<List<string>> FetchClientRedirectUrisAsync(string scheme, string clientId);
        Task<List<string>> FetchAllowedProtocolParamatersAsync(string scheme);
        Task<ClientRecord> FetchClientRecordAsync(string clientId);
    }
}
