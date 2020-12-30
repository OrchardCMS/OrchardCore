# CORS (`OrchardCore.Cors`)

CORS stands for Cross-Origin Resource Sharing.  
Modern browsers do not allow script execution from a different domain that served the scripts.  
This restriction is called the same-origin policy.  
In order to tell the browser to be less strict, we can allow some exceptions configured in the CORS module.

For more information, see https://docs.microsoft.com/en-us/aspnet/core/security/cors and https://developer.mozilla.org/nl/docs/Web/HTTP/CORS.

!!! warning
    As AllowCredentials and AllowAnyOrigin are considered a security risk, policies containing BOTH these options will NOT be activated.