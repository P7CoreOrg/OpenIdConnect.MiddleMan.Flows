
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public class ClientRecord
    {
        public string Secret { get; set; }
        public List<string> RedirectUris { get; set; }
    }

    public class OIDCSchemeRecord
    {
        public Dictionary<string, ClientRecord> ClientRecords { get; set; }
        public List<string> AllowedProtocolParamaters { get; set; }
    }

    public class InMemoryClientSecretStore: IClientSecretStore
    {
        private Dictionary<string, OIDCSchemeRecord> _records;

        public InMemoryClientSecretStore(Dictionary<string, OIDCSchemeRecord> records)
        {
            _records = records;
        }

        public Task<List<string>> FetchAllowedProtocolParamatersAsync(string scheme)
        {
            List<string> result = null;
            OIDCSchemeRecord record = null;
            if (_records.TryGetValue(scheme, out record))
            {
                result = record.AllowedProtocolParamaters;
            }
            
            return Task.FromResult(result??new List<string>());
        }

        public Task<ClientRecord> FetchClientRecordAsync(string clientId)
        {
            var record = (from item in _records
                          from r in item.Value.ClientRecords
                          where r.Key == clientId
                          select r.Value).FirstOrDefault();
            return Task.FromResult(record);
        }

        public Task<List<string>> FetchClientRedirectUrisAsync(string scheme, string clientId)
        {
            List<string> result = null;
            OIDCSchemeRecord schemeRecord = null;
            if (_records.TryGetValue(scheme, out schemeRecord))
            {
                ClientRecord clientRecord = null;
                if (schemeRecord.ClientRecords.TryGetValue(clientId, out clientRecord))
                {
                    result = clientRecord.RedirectUris;
                }
            }
            return Task.FromResult(result);
        }

        public Task<string> FetchClientSecretAsync(string scheme, string clientId)
        {
            string result = null;
            OIDCSchemeRecord schemeRecord = null;
            if(_records.TryGetValue(scheme,out schemeRecord))
            {
                ClientRecord clientRecord = null;
                if (schemeRecord.ClientRecords.TryGetValue(clientId, out clientRecord))
                {
                    result = clientRecord.Secret;
                }
            }
            return Task.FromResult(result);
        }
    }
}
