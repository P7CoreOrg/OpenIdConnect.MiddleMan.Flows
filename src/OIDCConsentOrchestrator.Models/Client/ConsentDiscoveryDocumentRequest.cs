namespace OIDCConsentOrchestrator.Models.Client
{
    public class ConsentDiscoveryDocumentRequest: ProtocolRequest 
    {
         
        /// <summary>
        /// Gets or sets the policy.
        /// </summary>
        /// <value>
        /// The policy.
        /// </value>
        public ConsentDiscoveryPolicy Policy { get; set; } = new ConsentDiscoveryPolicy();
    }
}
