# OpenIdConnect.MiddleMan.Flows
Sometimes just doing a login isn't enough

The OIDCPipeline.Core DiscoveryEndpoint calls googles [Discovery Document](https://accounts.google.com/.well-known/openid-configuration), replaces the authorization_endpoint with our own.  We are telling clients that we are the authority and thanks to the fact that the response lets us point to other endpoints helps.  

We are repacing the following;  
**authorization_endpoint** and **token_endpoint**  

The authorization_endpoint captures the original request, and the token_endpoint only supports the authorization_code flow.  


```
public async Task<IEndpointResult> ProcessAsync(HttpContext context)
{
    _logger.LogTrace("Processing discovery request.");
    // validate HTTP
    if (!HttpMethods.IsGet(context.Request.Method))
    {
        _logger.LogWarning("Discovery endpoint only supports GET requests");
        return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
    }
    _logger.LogDebug("Start discovery request");

    var response = await _downstreamDiscoveryCache.GetAsync();
    var downstreamStuff = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Raw);
    downstreamStuff["authorization_endpoint"]
       = $"{context.Request.Scheme}://{context.Request.Host}/connect/authorize";
    downstreamStuff["token_endpoint"]
      = $"{context.Request.Scheme}://{context.Request.Host}/connect/token";
    return new DiscoveryDocumentResult(downstreamStuff, _options.Discovery.ResponseCacheInterval);

}
```  

## Configuration
When we set up [google developer credentitals](https://developers.google.com/identity/protocols/OpenIDConnect) we make sure that the redirect uri points back to the middle man.  So the client_id/client_secret belongs to the client, but the middle man is task with custodial duties. 










