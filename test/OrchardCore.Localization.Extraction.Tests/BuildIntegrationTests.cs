using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace OrchardCore.Localization.Extraction.Tests;

[CollectionDefinition("Localization builds", DisableParallelization = true)]
public sealed class LocalizationBuildCollection;

[Collection("Localization builds")]
public sealed class BuildIntegrationTests
{
    [Fact]
    public async Task Build_CatalogLifecycle_PreservesResourcesAndInvalidatesInputs()
    {
        using var workspace = new TestWorkspace();
        var project = CopyFixture(workspace);
        var catalog = Path.Combine(workspace.DirectoryPath, "obj/Debug/net10.0/Localization/RazorModule.pot");
        await BuildAsync(project);
        var assembly = Path.Combine(workspace.DirectoryPath, "bin/Debug/net10.0/RazorModule.dll");
        var text = await File.ReadAllTextAsync(catalog, TestContext.Current.CancellationToken);
        Assert.Equal(text, ReadResource(assembly, "RazorModule.Localization.pot"));
        Assert.Contains("Fixture.Messages", text);
        Assert.Contains("RazorModule.Views.Index", text);
        Assert.Contains("RazorModule.Views.Nested", text);
        AssertNoToolingReferences(assembly);

        var timestamp = File.GetLastWriteTimeUtc(catalog);
        Assert.Contains("up to date", await BuildAsync(project));
        Assert.Equal(timestamp, File.GetLastWriteTimeUtc(catalog));
        await File.AppendAllTextAsync(Path.Combine(workspace.DirectoryPath, "Messages.cs"), "\n// A comment-only edit.\n", TestContext.Current.CancellationToken);
        Assert.Contains("3 messages", await BuildAsync(project));
        Assert.Equal(timestamp, File.GetLastWriteTimeUtc(catalog));

        workspace.Write("Added.cs", "using Microsoft.Extensions.Localization; public class Added { public string Get(IStringLocalizer<Added> words) => words[\"Added message\"]; }");
        await BuildAsync(project);
        Assert.Contains("Added message", await File.ReadAllTextAsync(catalog, TestContext.Current.CancellationToken));
        File.Delete(Path.Combine(workspace.DirectoryPath, "Added.cs"));
        await BuildAsync(project);
        Assert.DoesNotContain("Added message", await File.ReadAllTextAsync(catalog, TestContext.Current.CancellationToken));

        var requestTimestamp = File.GetLastWriteTimeUtc(catalog + ".request");
        var designTime = await RunDotnetAsync("msbuild", project, "-t:OrchardCoreGenerateLocalizationCatalog", "-p:DesignTimeBuild=true", "--nologo", "-v:quiet");
        Assert.Equal(0, designTime.ExitCode);
        Assert.Equal(requestTimestamp, File.GetLastWriteTimeUtc(catalog + ".request"));
        await RequireSuccessAsync("clean", project, "--tl:off", "--nologo", "-v:quiet");
        Assert.False(File.Exists(catalog));
        await BuildAsync(project);
        Assert.Equal(text, ReadResource(assembly, "RazorModule.Localization.pot"));
    }

    [Fact]
    public async Task Build_CachedWarningsAndStrictMode_ReplaysDiagnosticsAndFailsStrictBuild()
    {
        using var workspace = new TestWorkspace();
        var project = CopyFixture(workspace);
        workspace.Write("Dynamic.cs", "using Microsoft.Extensions.Localization; public class Dynamic { public string Get(IStringLocalizer<Dynamic> words, string key) => words[key]; }");
        Assert.Contains("OCLOC001", await BuildAsync(project));
        var cached = await BuildAsync(project);
        Assert.Contains("up to date", cached);
        Assert.Contains("OCLOC001", cached);
        var strict = await RunDotnetAsync("build", project, "--tl:off", "--nologo", "-v:minimal", "-p:LocalizationStrict=true");
        Assert.NotEqual(0, strict.ExitCode);
        Assert.Contains("OCLOC001", strict.Output);
    }

    [Fact]
    public async Task Build_SeparateProjectsInParallel_ProducesIndependentCatalogs()
    {
        using var first = new TestWorkspace();
        using var second = new TestWorkspace();
        var projects = new[] { CopyFixture(first), CopyFixture(second) };
        await Task.WhenAll(projects.Select(BuildAsync));
        var relative = "obj/Debug/net10.0/Localization/RazorModule.pot";
        Assert.Equal(await File.ReadAllTextAsync(Path.Combine(first.DirectoryPath, relative), TestContext.Current.CancellationToken), await File.ReadAllTextAsync(Path.Combine(second.DirectoryPath, relative), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Pack_MultiTargetConsumer_EmbedsCatalogsWithoutRuntimeTooling()
    {
        using var workspace = new TestWorkspace();
        var packageProject = Path.Combine(TestWorkspace.RepositoryRoot, "src/OrchardCore/OrchardCore.Localization.Build/OrchardCore.Localization.Build.csproj");
        var feed = Path.Combine(workspace.DirectoryPath, "feed");
        const string version = "0.0.0-localizationtests";
        await RequireSuccessAsync("pack", packageProject, "--tl:off", "--nologo", "-c", "Debug", "-o", feed, "-p:PackageVersion=" + version, "-v:quiet");
        using (var archive = ZipFile.OpenRead(Directory.GetFiles(feed, "*.nupkg").Single()))
        {
            Assert.Contains(archive.Entries, entry => entry.FullName == "build/OrchardCore.Localization.Build.targets");
            Assert.Contains(archive.Entries, entry => entry.FullName == "tools/net10.0/any/OrchardCore.Localization.Tools.dll");
            Assert.DoesNotContain(archive.Entries, entry => entry.FullName.StartsWith("lib/", StringComparison.Ordinal));
            using var reader = new StreamReader(archive.Entries.Single(entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open());
            var manifest = XDocument.Parse(await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
            Assert.DoesNotContain(manifest.Descendants(), element => element.Name.LocalName == "dependencies");
        }

        var cache = System.Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".nuget/packages");
        var consumer = workspace.Write("Consumer/Consumer.csproj", $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>netstandard2.0;net10.0</TargetFrameworks>
                <LangVersion>14.0</LangVersion>
                <RestoreAdditionalProjectSources>{{SecurityElement.Escape(feed)}}</RestoreAdditionalProjectSources>
                <RestorePackagesPath>{{SecurityElement.Escape(Path.Combine(workspace.DirectoryPath, "packages"))}}</RestorePackagesPath>
                <RestoreFallbackFolders>{{SecurityElement.Escape(cache)}}</RestoreFallbackFolders>
              </PropertyGroup>
              <ItemGroup>
                <PackageVersion Include="OrchardCore.Localization.Build" Version="{{version}}" />
                <PackageVersion Include="Microsoft.Extensions.Localization.Abstractions" Version="10.0.12" />
                <PackageReference Include="OrchardCore.Localization.Build" PrivateAssets="all" />
                <PackageReference Include="Microsoft.Extensions.Localization.Abstractions" />
              </ItemGroup>
            </Project>
            """);
        workspace.Write("Consumer/Messages.cs", "using Microsoft.Extensions.Localization; public class Messages { public string Get(IStringLocalizer<Messages> words) => words[\"Packaged message\"]; }");
        await BuildAsync(consumer);
        foreach (var framework in new[] { "netstandard2.0", "net10.0" })
        {
            var assembly = Path.Combine(workspace.DirectoryPath, "Consumer/bin/Debug", framework, "Consumer.dll");
            Assert.Contains("Packaged message", ReadResource(assembly, "Consumer.Localization.pot"));
            AssertNoToolingReferences(assembly);
        }

        var publish = Path.Combine(workspace.DirectoryPath, "publish");
        await RequireSuccessAsync("publish", consumer, "--tl:off", "--nologo", "-f", "net10.0", "-c", "Debug", "-o", publish, "-v:quiet");
        Assert.Contains("Packaged message", ReadResource(Path.Combine(publish, "Consumer.dll"), "Consumer.Localization.pot"));
        Assert.DoesNotContain(Directory.GetFiles(publish, "*.dll"), path => Path.GetFileName(path).Contains("CodeAnalysis", StringComparison.Ordinal) || Path.GetFileName(path).Contains("Localization.Tools", StringComparison.Ordinal));
    }

    private static string CopyFixture(TestWorkspace workspace)
    {
        var source = Path.Combine(TestWorkspace.RepositoryRoot, "test/OrchardCore.Localization.Extraction.Tests/Fixtures/RazorModule");
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(source, file);
            if (relative.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            {
                continue;
            }

            workspace.Write(relative, File.ReadAllText(file));
        }

        var project = Path.Combine(workspace.DirectoryPath, "RazorModule.csproj");
        var buildDirectory = Path.Combine(TestWorkspace.RepositoryRoot, "src/OrchardCore/OrchardCore.Localization.Build");
        var content = File.ReadAllText(project).Replace("$(MSBuildThisFileDirectory)../../../../src/OrchardCore/OrchardCore.Localization.Build", SecurityElement.Escape(buildDirectory), StringComparison.Ordinal);
        File.WriteAllText(project, content);
        return project;
    }

    private static Task<string> BuildAsync(string project)
        => RequireSuccessAsync("build", project, "--tl:off", "--nologo", "-v:minimal");

    private static async Task<string> RequireSuccessAsync(params string[] arguments)
    {
        var result = await RunDotnetAsync(arguments);
        Assert.True(result.ExitCode == 0, result.Output);
        return result.Output;
    }

    private static async Task<(int ExitCode, string Output)> RunDotnetAsync(params string[] arguments)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(3));
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, WorkingDirectory = TestWorkspace.RepositoryRoot };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync(timeout.Token);
        var error = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(true);
            throw;
        }

        return (process.ExitCode, await output + await error);
    }

    private static string ReadResource(string path, string name)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var resource = metadata.ManifestResources.Select(metadata.GetManifestResource).Single(resource => metadata.GetString(resource.Name) == name);
        Assert.True(resource.Implementation.IsNil);
        var data = pe.GetSectionData(pe.PEHeaders.CorHeader!.ResourcesDirectory.RelativeVirtualAddress).GetContent().AsSpan();
        var offset = checked((int)resource.Offset);
        var length = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, 4));
        return Encoding.UTF8.GetString(data.Slice(offset + 4, length));
    }

    private static void AssertNoToolingReferences(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var names = metadata.AssemblyReferences.Select(handle => metadata.GetString(metadata.GetAssemblyReference(handle).Name));
        Assert.DoesNotContain(names, name => name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal) || name.StartsWith("OrchardCore.Localization.Tools", StringComparison.Ordinal) || name.StartsWith("OrchardCore.Localization.Extraction", StringComparison.Ordinal));
    }
}
