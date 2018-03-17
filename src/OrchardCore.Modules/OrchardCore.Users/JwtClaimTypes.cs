// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityModel
{
    public static class JwtClaimTypes
    {
        /// <summary>Unique Identifier for the End-User at the Issuer.</summary>
        public const string Subject = "sub";

        /// <summary>End-User's full name in displayable form including all name parts, possibly including titles and suffixes, ordered according to the End-User's locale and preferences.</summary>
        public const string Name = "name";

        /// <summary>Given name(s) or first name(s) of the End-User. Note that in some cultures, people can have multiple given names; all can be present, with the names being separated by space characters.</summary>
        public const string GivenName = "given_name";

        /// <summary>Surname(s) or last name(s) of the End-User. Note that in some cultures, people can have multiple family names or no family name; all can be present, with the names being separated by space characters.</summary>
        public const string FamilyName = "family_name";

        /// <summary>Middle name(s) of the End-User. Note that in some cultures, people can have multiple middle names; all can be present, with the names being separated by space characters. Also note that in some cultures, middle names are not used.</summary>
        public const string MiddleName = "middle_name";

        /// <summary>Casual name of the End-User that may or may not be the same as the given_name. For instance, a nickname value of Mike might be returned alongside a given_name value of Michael.</summary>
        public const string NickName = "nickname";

        /// <summary>Shorthand name by which the End-User wishes to be referred to at the RP, such as janedoe or j.doe. This value MAY be any valid JSON string including special characters such as @, /, or whitespace. The relying party MUST NOT rely upon this value being unique</summary>
        /// <remarks>The RP MUST NOT rely upon this value being unique, as discussed in http://openid.net/specs/openid-connect-basic-1_0-32.html#ClaimStability </remarks>
        public const string PreferredUserName = "preferred_username";

        /// <summary>URL of the End-User's profile page. The contents of this Web page SHOULD be about the End-User.</summary>
        public const string Profile = "profile";

        /// <summary>URL of the End-User's profile picture. This URL MUST refer to an image file (for example, a PNG, JPEG, or GIF image file), rather than to a Web page containing an image.</summary>
        /// <remarks>Note that this URL SHOULD specifically reference a profile photo of the End-User suitable for displaying when describing the End-User, rather than an arbitrary photo taken by the End-User.</remarks>
        public const string Picture = "picture";

        /// <summary>URL of the End-User's Web page or blog. This Web page SHOULD contain information published by the End-User or an organization that the End-User is affiliated with.</summary>
        public const string WebSite = "website";

        /// <summary>End-User's preferred e-mail address. Its value MUST conform to the RFC 5322 [RFC5322] addr-spec syntax. The relying party MUST NOT rely upon this value being unique</summary>
        public const string Email = "email";

        /// <summary>"true" if the End-User's e-mail address has been verified; otherwise "false".</summary>
        ///  <remarks>When this Claim Value is "true", this means that the OP took affirmative steps to ensure that this e-mail address was controlled by the End-User at the time the verification was performed. The means by which an e-mail address is verified is context-specific, and dependent upon the trust framework or contractual agreements within which the parties are operating.</remarks>
        public const string EmailVerified = "email_verified";

        /// <summary>End-User's gender. Values defined by this specification are "female" and "male". Other values MAY be used when neither of the defined values are applicable.</summary>
        public const string Gender = "gender";

        /// <summary>End-User's birthday, represented as an ISO 8601:2004 [ISO8601‑2004] YYYY-MM-DD format. The year MAY be 0000, indicating that it is omitted. To represent only the year, YYYY format is allowed. Note that depending on the underlying platform's date related function, providing just year can result in varying month and day, so the implementers need to take this factor into account to correctly process the dates.</summary>
        public const string BirthDate = "birthdate";

        /// <summary>String from the time zone database (http://www.twinsun.com/tz/tz-link.htm) representing the End-User's time zone. For example, Europe/Paris or America/Los_Angeles.</summary>
        public const string ZoneInfo = "zoneinfo";

        /// <summary>End-User's locale, represented as a BCP47 [RFC5646] language tag. This is typically an ISO 639-1 Alpha-2 [ISO639‑1] language code in lowercase and an ISO 3166-1 Alpha-2 [ISO3166‑1] country code in uppercase, separated by a dash. For example, en-US or fr-CA. As a compatibility note, some implementations have used an underscore as the separator rather than a dash, for example, en_US; Relying Parties MAY choose to accept this locale syntax as well.</summary>
        public const string Locale = "locale";

        /// <summary>End-User's preferred telephone number. E.164 (https://www.itu.int/rec/T-REC-E.164/e) is RECOMMENDED as the format of this Claim, for example, +1 (425) 555-1212 or +56 (2) 687 2400. If the phone number contains an extension, it is RECOMMENDED that the extension be represented using the RFC 3966 [RFC3966] extension syntax, for example, +1 (604) 555-1234;ext=5678.</summary>
        public const string PhoneNumber = "phone_number";

        /// <summary>True if the End-User's phone number has been verified; otherwise false. When this Claim Value is true, this means that the OP took affirmative steps to ensure that this phone number was controlled by the End-User at the time the verification was performed.</summary>
        /// <remarks>The means by which a phone number is verified is context-specific, and dependent upon the trust framework or contractual agreements within which the parties are operating. When true, the phone_number Claim MUST be in E.164 format and any extensions MUST be represented in RFC 3966 format.</remarks>
        public const string PhoneNumberVerified = "phone_number_verified";

        /// <summary>End-User's preferred postal address. The value of the address member is a JSON structure containing some or all of the members defined in http://openid.net/specs/openid-connect-basic-1_0-32.html#AddressClaim </summary>
        public const string Address = "address";

        /// <summary>Audience(s) that this ID Token is intended for. It MUST contain the OAuth 2.0 client_id of the Relying Party as an audience value. It MAY also contain identifiers for other audiences. In the general case, the aud value is an array of case sensitive strings. In the common special case when there is one audience, the aud value MAY be a single case sensitive string.</summary>
        public const string Audience = "aud";

        /// <summary>Issuer Identifier for the Issuer of the response. The iss value is a case sensitive URL using the https scheme that contains scheme, host, and optionally, port number and path components and no query or fragment components.</summary>
        public const string Issuer = "iss";

        /// <summary>The time before which the JWT MUST NOT be accepted for processing, specified as the number of seconds from 1970-01-01T0:0:0Z</summary>
        public const string NotBefore = "nbf";

        /// <summary>The exp (expiration time) claim identifies the expiration time on or after which the token MUST NOT be accepted for processing, specified as the number of seconds from 1970-01-01T0:0:0Z</summary>
        public const string Expiration = "exp";

        /// <summary>Time the End-User's information was last updated. Its value is a JSON number representing the number of seconds from 1970-01-01T0:0:0Z as measured in UTC until the date/time.</summary>
        public const string UpdatedAt = "updated_at";

        /// <summary>The iat (issued at) claim identifies the time at which the JWT was issued, , specified as the number of seconds from 1970-01-01T0:0:0Z</summary>
        public const string IssuedAt = "iat";

        /// <summary>Authentication Methods References. JSON array of strings that are identifiers for authentication methods used in the authentication.</summary>
        public const string AuthenticationMethod = "amr";

        /// <summary>Session identifier. This represents a Session of an OP at an RP to a User Agent or device for a logged-in End-User. Its contents are unique to the OP and opaque to the RP.</summary>
        public const string SessionId = "sid";

        /// <summary>
        /// Authentication Context Class Reference. String specifying an Authentication Context Class Reference value that identifies the Authentication Context Class that the authentication performed satisfied. 
        /// The value "0" indicates the End-User authentication did not meet the requirements of ISO/IEC 29115 level 1. 
        /// Authentication using a long-lived browser cookie, for instance, is one example where the use of "level 0" is appropriate. 
        /// Authentications with level 0 SHOULD NOT be used to authorize access to any resource of any monetary value.
        ///  (This corresponds to the OpenID 2.0 PAPE nist_auth_level 0.) 
        /// An absolute URI or an RFC 6711 registered name SHOULD be used as the acr value; registered names MUST NOT be used with a different meaning than that which is registered. 
        /// Parties using this claim will need to agree upon the meanings of the values used, which may be context-specific. 
        /// The acr value is a case sensitive string.
        /// </summary>
        public const string AuthenticationContextClassReference = "acr";

        /// <summary>Time when the End-User authentication occurred. Its value is a JSON number representing the number of seconds from 1970-01-01T0:0:0Z as measured in UTC until the date/time. When a max_age request is made or when auth_time is requested as an Essential Claim, then this Claim is REQUIRED; otherwise, its inclusion is OPTIONAL.</summary>
        public const string AuthenticationTime = "auth_time";

        /// <summary>The party to which the ID Token was issued. If present, it MUST contain the OAuth 2.0 Client ID of this party. This Claim is only needed when the ID Token has a single audience value and that audience is different than the authorized party. It MAY be included even when the authorized party is the same as the sole audience. The azp value is a case sensitive string containing a StringOrURI value.</summary>
        public const string AuthorizedParty = "azp";

        /// <summary> Access Token hash value. Its value is the base64url encoding of the left-most half of the hash of the octets of the ASCII representation of the access_token value, where the hash algorithm used is the hash algorithm used in the alg Header Parameter of the ID Token's JOSE Header. For instance, if the alg is RS256, hash the access_token value with SHA-256, then take the left-most 128 bits and base64url encode them. The at_hash value is a case sensitive string.</summary>
        public const string AccessTokenHash = "at_hash";

        /// <summary>Code hash value. Its value is the base64url encoding of the left-most half of the hash of the octets of the ASCII representation of the code value, where the hash algorithm used is the hash algorithm used in the alg Header Parameter of the ID Token's JOSE Header. For instance, if the alg is HS512, hash the code value with SHA-512, then take the left-most 256 bits and base64url encode them. The c_hash value is a case sensitive string.</summary>
        public const string AuthorizationCodeHash = "c_hash";

        /// <summary>String value used to associate a Client session with an ID Token, and to mitigate replay attacks. The value is passed through unmodified from the Authentication Request to the ID Token. If present in the ID Token, Clients MUST verify that the nonce Claim Value is equal to the value of the nonce parameter sent in the Authentication Request. If present in the Authentication Request, Authorization Servers MUST include a nonce Claim in the ID Token with the Claim Value being the nonce value sent in the Authentication Request. Authorization Servers SHOULD perform no other processing on nonce values used. The nonce value is a case sensitive string.</summary>
        public const string Nonce = "nonce";

        /// <summary>JWT ID. A unique identifier for the token, which can be used to prevent reuse of the token. These tokens MUST only be used once, unless conditions for reuse were negotiated between the parties; any such negotiation is beyond the scope of this specification.</summary>
        public const string JwtId = "jti";

        /// <summary>OAuth 2.0 Client Identifier valid at the Authorization Server.</summary>
        public const string ClientId = "client_id";

        /// <summary>OpenID Connect requests MUST contain the "openid" scope value. If the openid scope value is not present, the behavior is entirely unspecified. Other scope values MAY be present. Scope values used that are not understood by an implementation SHOULD be ignored.</summary>
        public const string Scope = "scope";

        public const string Id = "id";
        public const string IdentityProvider = "idp";
        public const string Role = "role";
        public const string ReferenceTokenId = "reference_token_id";
    }
}