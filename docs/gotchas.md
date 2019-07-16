# Gotchas

Lots of redirects going on here and then there area backchannel calls back to the IDP as a result of that IDP redirecting back to the Orchestrator.  

You **CAN NOT** rely on your HTTPCONTEXT referencing your frontend REQUEST, hence you can't get to cookies.

1. When the initial request comes in via the authorization_endpoint I drop a tracking cookie with a GUID.  This GUID aka the key, is used to store the original request and the downstream response.  
2. When the downstream IDP redirects back, we loose HTTPContext due to the async stuff going on.  To get our KEY back we pass it to the down stream authorization_endpoint as a **state** paramater.  When we get the redirect and subsequent backchannel calls are made to the IDP, we have access to the **state**, hence we can then store the downstream response in the right context.
3. This stored key becomes the **CODE** we respond back to the client app.

So in summary, cookies are used in the initial ingres, then we transition to **state** passing, and then finally we go back the the cookie for our final redirect to the waiting client app.  


