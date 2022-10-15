using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build : NukeBuild
{
    const string DefaultTargetFramework = "net6.0";

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Whether to run functional tests, defaults to true when running on Linux platfrom")]
    readonly bool IncludeFunctionalTests = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
            );
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetFramework(DefaultTargetFramework)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
        });

    Target Test => _ => _
        .DependsOn(UnitTest, FunctionalTest);

    Target UnitTest => _ => _
        .After(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution.GetProject("OrchardCore.Tests"))
                .SetFramework(DefaultTargetFramework)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
            );
        });

    Target FunctionalTest => _ => _
        .After(Compile)
        .OnlyWhenDynamic(() => IncludeFunctionalTests)
        .Produces(RootDirectory / "test/OrchardCore.Tests.Functional/cms-tests/cypress/screenshots/*.*", RootDirectory / "src/OrchardCore.Cms.Web/App_Data/logs/*.*")
        .Executes(() =>
        {
            var project = Solution.GetProject("OrchardCore.Tests.Functional");
            NpmInstall(settings => settings.SetProcessWorkingDirectory(project.Directory));
            NpmRun(settings => settings.SetProcessWorkingDirectory(project.Directory).SetCommand("cms:test"));
            NpmRun(settings => settings.SetProcessWorkingDirectory(project.Directory).SetCommand("mvc:test"));
        });

    Target Pack => _ => _
        .After(Test, Compile)
        .Produces(ArtifactsDirectory / "*.*")
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);

            DotNetPack(s => s
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetDeterministic(IsServerBuild)
                .SetContinuousIntegrationBuild(IsServerBuild)
            );
        });
}
