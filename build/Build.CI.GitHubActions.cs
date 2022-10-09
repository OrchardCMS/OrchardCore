using Nuke.Common.CI.GitHubActions;

[GitHubActions(
    "pr_ci",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    //GitHubActionsImage.MacOsLatest,
    OnPullRequestBranches = new[] { "master", "main" },
    OnPullRequestIncludePaths = new[] { "**/*.*" },
    OnPullRequestExcludePaths = new[] { "**/*.md" },
    PublishArtifacts = true,
    InvokedTargets = new[] { nameof(Compile), nameof(Test), nameof(Pack) },
    CacheKeyFiles = new[] { "global.json", "src/**/*.csproj", "src/**/package.json" },
    AutoGenerate = false), // could be generated
]
public partial class Build
{
}
