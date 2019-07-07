using System;
using System.Collections.Generic;
using System.Text;

namespace OIDCPipeline.Core.Configuration
{
    public class MemoryCacheOIDCPipelineStoreOptions
    {
        public int ExpirationMinutes { get; set; } = 30;
    }
}
