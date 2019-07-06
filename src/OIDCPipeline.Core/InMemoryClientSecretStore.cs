
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public class InMemoryClientSecretStore: IClientSecretStore
    {
        private Dictionary<string, string> _clientSecrets;

        public InMemoryClientSecretStore(Dictionary<string,string> clientSecrets)
        {
            _clientSecrets = clientSecrets;
           
        }

        public async Task<string> FetchClientSecretAsync(string scheme, string clientId)
        {
            string secret = null;
            _clientSecrets.TryGetValue(clientId, out secret);
            return secret;
         
        }
    }
}
