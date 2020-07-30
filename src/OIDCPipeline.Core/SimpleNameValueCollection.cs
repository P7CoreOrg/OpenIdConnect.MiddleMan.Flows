using System.Collections.Generic;

namespace OIDCPipeline.Core
{
    public class SimpleNameValueCollection : Dictionary<string, string>
    {
        public KeyCollection AllKeys
        {
            get
            {
                return Keys;
            }
        }
        public string Get(string name)
        {
            string value = null;
            if (TryGetValue(name, out value))
            {
                return value;
            }
            return null;

        }
    }
}
