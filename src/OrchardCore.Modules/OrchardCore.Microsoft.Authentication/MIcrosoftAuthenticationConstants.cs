/*
	*Azure Active Directory Enterprise Identity Management (Azure AD)
	provides single sign-on and multi-factor authentication
	to help protect users from 99.9% of cyber attacks.
	-------------------------------------------------------
	string constants - abbreviations for path at application structure
*/

namespace OrchardCore.Microsoft.Authentication
{
    public static class MicrosoftAuthenticationConstants
    {
        public static class Features
        {
            public const string MicrosoftAccount = "OrchardCore.Microsoft.Authentication.MicrosoftAccount";
            public const string AAD = "OrchardCore.Microsoft.Authentication.AzureAD";
        }
    }
}
