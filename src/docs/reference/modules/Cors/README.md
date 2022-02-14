# CORS (`OrchardCore.Cors`)

CORS stands for Cross-Origin Resource Sharing.  
Modern browsers do not allow script execution from a different domain that serves the scripts.
This restriction is called the same-origin policy.  
In order to tell the browser to be less strict, we can allow some exceptions configured in the CORS module.

For more information, see https://docs.microsoft.com/en-us/aspnet/core/security/cors and https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS.

!!! warning
    As using AllowCredentials and AllowAnyOrigin at the same time is considered as a security risk, policies containing BOTH these options will NOT be activated.

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/OYXFvKWyVGo" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
