namespace OIDCPipeline.Core.Configuration
{
    public class DiscoveryOptions
    {

        public int? ResponseCacheInterval { get; set; } = null;


    }
    public class OIDCPipelineOptions
    {
        public string DownstreamAuthority { get; set; } = "https://accounts.google.com";
        public string PostAuthorizeHookRedirectUrl { get; set; } = "/";
        public string PostAuthorizeHookErrorRedirectUrl { get; set; } = "/";
        public DiscoveryOptions Discovery { get; set; } = new DiscoveryOptions();

    }
}
