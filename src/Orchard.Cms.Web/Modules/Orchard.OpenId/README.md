# Orchard.OpenId

Orchard.OpenId provides an implementation of an OpenID Connect server based on [OpenIddict](https://github.com/openiddict/openiddict-core) library. 
It allows Orchard 2 to act as identity provider to support token authentication without the need of an external identity provider.
So, Orchard 2 can be used also as an identity provider for centralizing the user access permissions to external applications not only to Orchard 2 services.

Flows supported: [code/implicit/hybrid flows](http://openid.net/specs/openid-connect-core-1_0.html) and [client credentials/resource owner password grants](https://tools.ietf.org/html/rfc6749).



## Configuration

Configuration can be set through Open Id menu in the admin dashboard and also through a recipe step.

Available settings are:
+ Testing Mode: Enabling Testing mode, removes the need of providing a certificate for signing tokens providing an ephemeral key. Also removes the requirement of using an HTTPS for issuing tokens.
+ Token Format: there are two options:
  + JWT: This format uses signed JWT standard tokens (not encrypted). It requires the SSL certificate being used is accepted as a trusted certificate by the client.
  + Encrypted: This format uses non standard opaque tokens encrypted by the ASP.NET data protection block. 
+ Authority: Orchard url used by orchard to act as an identity server.
+ Audiences: Urls of the resource servers for which the identity server issues valid JWT tokens.
+ Certificate Store Location: CurrentUser/LocalMachine https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storelocation(v=vs.110).aspx
+ Certificate Store Name: AddressBook/AuthRootCertificateAuthority/Disallowed/My/Root/TrustedPeople/TrustedPublisher https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx
+ Certificate Thumbprint: The thumbprint of the certificate (It is recommended to not use same certificate it is been used for SSL).

A sample of Open Id Settings recipe step:
```
{
      "name": "openidsettings",
      "TestingModeEnabled": false,
      "DefaultTokenFormat": "JWT", //JWT or Encrypted
      "Authority": "https://www.orchardproject.net",
      "Audiences": ["https://www.orchardproject.net","https://orchardharvest.org/"],
      "CertificateStoreLocation": "LocalMachine", //More info: https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storelocation(v=vs.110).aspx
      "CertificateStoreName": "My", //More info: https://msdn.microsoft.com/en-us/library/system.security.cryptography.x509certificates.storename(v=vs.110).aspx
      "CertificateThumbPrint": "27CCA66EF38EF46CD9022431FB1FF0F2DF5CA1D7"
}
```

### Client Open Id Apps Configuration

TO-DO: Complete this section when open id apps configuration final version is ready.