
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public class OIDCSchemeRecord
    {
        public Dictionary<string, string> ClientSecrets { get; set; }
        public List<string> AllowedProtocolParamaters { get; set; }
    }

    public class InMemoryClientSecretStore: IClientSecretStore
    {
        private Dictionary<string, OIDCSchemeRecord> _OIDCSchemeRecord;

        public InMemoryClientSecretStore(Dictionary<string, OIDCSchemeRecord> clientSecrets)
        {
            _OIDCSchemeRecord = clientSecrets;
           
        }

        public Task<List<string>> FetchAllowedProtocolParamatersAsync(string scheme)
        {
            List<string> result = null;
            OIDCSchemeRecord record = null;
            if (_OIDCSchemeRecord.TryGetValue(scheme, out record))
            {
                result = record.AllowedProtocolParamaters;
            }
            return Task.FromResult(result);
        }

        public Task<string> FetchClientSecretAsync(string scheme, string clientId)
        {
            string secret = null;
            OIDCSchemeRecord record = null;
            if(_OIDCSchemeRecord.TryGetValue(scheme,out record))
            {
                record.ClientSecrets.TryGetValue(clientId, out secret);
            }
            return Task.FromResult(secret);
        }
    }
}
