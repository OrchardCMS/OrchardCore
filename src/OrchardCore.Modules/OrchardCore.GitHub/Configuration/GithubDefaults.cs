namespace OrchardCore.GitHub.Configuration;

public static class GitHubDefaults
{
    public const string AuthenticationScheme = "GitHub";

    public const string DisplayName = "GitHub";

    public const string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";

    public const string TokenEndpoint = "https://github.com/login/oauth/access_token";

    public const string UserInformationEndpoint = "https://api.github.com/user";
}
