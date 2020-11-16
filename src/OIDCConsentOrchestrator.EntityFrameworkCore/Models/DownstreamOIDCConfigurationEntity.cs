using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIDCConsentOrchestrator.EntityFrameworkCore
{
    public class DownstreamOIDCConfigurationEntity : BaseEntity
    {
        public string Name { get; set; }
        [ForeignKey("DownstreamOIDCConfigurationFK")]
        public virtual ICollection<OIDCClientConfigurationEntity> OIDCClientConfigurations { get; set; }
 
    }
}
