namespace OrchardCore.GitHub.Recipes;

public sealed class GitHubLoginSettingsStepModel
{
    public string ConsumerKey { get; set; }

    public string ConsumerSecret { get; set; }

    public string CallbackPath { get; set; }
}
