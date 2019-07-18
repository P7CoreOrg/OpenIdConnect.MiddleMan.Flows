# OpenIdConnect.Orchestrator.Flows
Sometimes just doing a login isn't enough

There is login and then there are followup pages that have nothing to do with the IDP.  Just because someone has a valid account on some IDP (i.e. you are using google as your main idp), doesn't mean that the user has an account on your platform.  Those follow up "sign up" are not googles, they are yours.  This is where you vet the user and perhaps ask for money before you burn resources on your end to make a real account.  

This project demos that process and advertises itself as a compliant OIDC provider.  You are not and IDP, but an orchestrator whose interfrace to a mobile app is a standard that they can code against.

![PlantUML model](http://www.plantuml.com/plantuml/png/5SqngiCm383X_PtYzG2rnaAdW6cXT72kuCeY3jYIaMHo_JRJ3__oBUPPVVRsTzaPsomqjVrNzs5t0Cr7s7QlypED58MTs0DAX_KMHIdf1caGlqeKPa8FIR6IkMON3SycXq7FvgHG10tMTtnSpnt6QIx4vTSl)


The OIDCPipeline.Core DiscoveryEndpoint calls googles [Discovery Document](https://accounts.google.com/.well-known/openid-configuration), replaces the **authorization_endpoint** and **token_endpoint** with our own.  We are telling clients that we are the authority and thanks to the fact that the response lets us point to other endpoints helps.  

```
{
	"issuer": "https://accounts.google.com",
	"authorization_endpoint": "https://localhost:5001/connect/authorize",
	"token_endpoint": "https://localhost:5001/connect/token",
	"userinfo_endpoint": "https://openidconnect.googleapis.com/v1/userinfo",
	"revocation_endpoint": "https://oauth2.googleapis.com/revoke",
	"jwks_uri": "https://www.googleapis.com/oauth2/v3/certs",
	"response_types_supported": ["code", "token", "id_token", "code token", "code id_token", "token id_token", "code token id_token", "none"],
	"subject_types_supported": ["public"],
	"id_token_signing_alg_values_supported": ["RS256"],
	"scopes_supported": ["openid", "email", "profile"],
	"token_endpoint_auth_methods_supported": ["client_secret_post", "client_secret_basic"],
	"claims_supported": ["aud", "email", "email_verified", "exp", "family_name", "given_name", "iat", "iss", "locale", "name", "picture", "sub"],
	"code_challenge_methods_supported": ["plain", "S256"]
}
```

The authorization_endpoint captures the original request, and the token_endpoint only supports the authorization_code flow.  
**NOTE**: we are **NOT** a replacement for the IDP. So even though we changed the *token_endpoint*, we are **NOT** going to proxy calls to the downstream OIDC server.  This is a **authorization_code** only login flow service for clients.  


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










