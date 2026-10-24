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
    private const string Update = "-p:UpdateLocalizationCatalogs=true";

    [Fact]
    public async Task Build_WithoutTemplate_EmbedsNothingAndRunsNoExtraction()
    {
        using var workspace = new TestWorkspace();
        var project = CopyFixture(workspace);

        var output = await BuildAsync(project);

        Assert.DoesNotContain("Localization catalog", output);
        Assert.False(File.Exists(Path.Combine(workspace.DirectoryPath, "RazorModule.pot")));
        Assert.False(HasResource(Path.Combine(workspace.DirectoryPath, "bin/Debug/net10.0/RazorModule.dll"), "RazorModule.Localization.pot"));
    }

    [Fact]
    public async Task Update_TemplateLifecycle_WritesNextToProjectAndBuildEmbedsIt()
    {
        using var workspace = new TestWorkspace();
        var project = CopyFixture(workspace);
        var template = Path.Combine(workspace.DirectoryPath, "RazorModule.pot");
        var stamp = Path.Combine(workspace.DirectoryPath, "obj/Debug/net10.0/Localization/RazorModule.pot.stamp.json");
        var assembly = Path.Combine(workspace.DirectoryPath, "bin/Debug/net10.0/RazorModule.dll");

        Assert.Contains("3 messages in RazorModule.pot", await BuildAsync(project, Update));
        var text = await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken);
        Assert.Contains("Fixture.Messages", text);
        Assert.Contains("RazorModule.Views.Index", text);
        Assert.Contains("RazorModule.Views.Nested", text);
        Assert.Contains("#: Messages.cs\n", text);
        Assert.DoesNotContain("#: Messages.cs:", text);
        Assert.Equal(text, ReadResource(assembly, "RazorModule.Localization.pot"));
        AssertNoToolingReferences(assembly);
        Assert.True(File.Exists(stamp));

        var timestamp = File.GetLastWriteTimeUtc(template);
        Assert.Contains("up to date", await BuildAsync(project, Update));
        Assert.Equal(timestamp, File.GetLastWriteTimeUtc(template));
        await File.AppendAllTextAsync(Path.Combine(workspace.DirectoryPath, "Messages.cs"), "\n// A comment-only edit.\n", TestContext.Current.CancellationToken);
        Assert.Contains("3 messages", await BuildAsync(project, Update));
        Assert.Equal(timestamp, File.GetLastWriteTimeUtc(template));

        workspace.Write("Added.cs", "using Microsoft.Extensions.Localization; public class Added { public string Get(IStringLocalizer<Added> words) => words[\"Added message\"]; }");
        await BuildAsync(project, Update);
        Assert.Contains("Added message", await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken));
        File.Delete(Path.Combine(workspace.DirectoryPath, "Added.cs"));
        Assert.Contains("3 messages", await RequireSuccessAsync("msbuild", project, "-restore", "-t:UpdateLocalizationCatalog", "--tl:off", "--nologo", "-v:minimal"));
        Assert.Equal(text, await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken));

        // A build without the update embeds the checked-in template as it is, even when the sources changed.
        workspace.Write("Added.cs", "using Microsoft.Extensions.Localization; public class Added { public string Get(IStringLocalizer<Added> words) => words[\"Not extracted yet\"]; }");
        Assert.DoesNotContain("Localization catalog", await BuildAsync(project));
        Assert.Equal(text, ReadResource(assembly, "RazorModule.Localization.pot"));

        var designTime = await RunDotnetAsync("msbuild", project, "-t:UpdateLocalizationCatalog", "-p:DesignTimeBuild=true", "--nologo", "-v:quiet");
        Assert.Equal(0, designTime.ExitCode);
        Assert.Equal(text, await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken));

        await RequireSuccessAsync("clean", project, "--tl:off", "--nologo", "-v:quiet");
        Assert.True(File.Exists(template));
        Assert.False(File.Exists(stamp));
        await BuildAsync(project);
        Assert.Equal(text, ReadResource(assembly, "RazorModule.Localization.pot"));
    }

    [Fact]
    public async Task Update_NoMessages_RemovesTemplateAndBuildEmbedsNothing()
    {
        using var workspace = new TestWorkspace();
        var project = CopyFixture(workspace);
        var template = Path.Combine(workspace.DirectoryPath, "RazorModule.pot");
        await BuildAsync(project, Update);
        Assert.True(File.Exists(template));

        workspace.Write("Messages.cs", "namespace Fixture; public class Messages { }");
        foreach (var view in Directory.GetFiles(Path.Combine(workspace.DirectoryPath, "Views")))
        {
            File.WriteAllText(view, "<p>Nothing to translate</p>");
        }

        Assert.Contains("removed", await BuildAsync(project, Update));
        Assert.False(File.Exists(template));
        Assert.Contains("up to date", await BuildAsync(project, Update));
        Assert.False(HasResource(Path.Combine(workspace.DirectoryPath, "bin/Debug/net10.0/RazorModule.dll"), "RazorModule.Localization.pot"));
    }

    [Fact]
    public async Task Update_Configurations_ProduceTheSameTemplate()
    {
        using var workspace = new TestWorkspace();
        var project = CopyFixture(workspace);
        workspace.Write("Conditional.cs", """
            using Microsoft.Extensions.Localization;
            public class Conditional
            {
                public string Get(IStringLocalizer<Conditional> words)
                {
            #if DEBUG
                    return words["Debug only message"];
            #else
                    return words["Release only message"];
            #endif
                }
            }
            """);
        var template = Path.Combine(workspace.DirectoryPath, "RazorModule.pot");

        await BuildAsync(project, Update);
        var debug = await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken);
        await BuildAsync(project, Update, "-c", "Release");
        var release = await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken);

        Assert.Equal(debug, release);
        Assert.DoesNotContain("Debug only message", debug);
        Assert.Contains("Release only message", debug);
    }

    [Fact]
    public async Task Build_CatalogCollection_CopiesCheckedInTemplates()
    {
        using var workspace = new TestWorkspace();
        var project = CopyFixture(workspace);
        var output = Path.Combine(workspace.DirectoryPath, "exported templates");
        var template = Path.Combine(workspace.DirectoryPath, "RazorModule.pot");
        var exported = Path.Combine(output, "net10.0/RazorModule.pot");
        var property = "-p:LocalizationCatalogOutputPath=" + output;
        await BuildAsync(project, property);
        Assert.False(Directory.Exists(output));

        await BuildAsync(project, Update);
        Assert.DoesNotContain("Localization catalog", await BuildAsync(project, property));
        Assert.Equal(await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken), await File.ReadAllTextAsync(exported, TestContext.Current.CancellationToken));
        Assert.Single(Directory.GetFiles(output, "*", SearchOption.AllDirectories));
        var timestamp = File.GetLastWriteTimeUtc(exported);
        await BuildAsync(project, property);
        Assert.Equal(timestamp, File.GetLastWriteTimeUtc(exported));

        Directory.Delete(output, true);
        await BuildAsync(project, property, "-p:GenerateLocalizationCatalog=false");
        Assert.False(Directory.Exists(output));
        await BuildAsync(project, "-p:LocalizationCatalogOutputPath=exported templates");
        Assert.True(File.Exists(exported));
    }

    [Fact]
    public async Task Update_ReferencedProjects_UpdatesAndEmbedsEveryTemplate()
    {
        using var application = new TestWorkspace();
        using var module = new TestWorkspace();
        var project = CopyFixture(application);
        var reference = CopyFixture(module);
        var definition = XDocument.Load(project);
        definition.Root!.Element("PropertyGroup")!.Add(new XElement("AssemblyName", "Application"));
        definition.Root.Add(new XElement("ItemGroup", new XElement("ProjectReference", new XAttribute("Include", reference))));
        definition.Save(project);

        await BuildAsync(project, Update);

        foreach (var (workspace, assemblyName) in new[] { (application, "Application"), (module, "RazorModule") })
        {
            var template = Path.Combine(workspace.DirectoryPath, assemblyName + ".pot");
            var assembly = Path.Combine(application.DirectoryPath, "bin/Debug/net10.0", assemblyName + ".dll");
            Assert.Equal(await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken), ReadResource(assembly, assemblyName + ".Localization.pot"));
        }
    }

    [Fact]
    public async Task Update_CachedWarningsAndStrictMode_ReplaysDiagnosticsAndFailsStrictUpdate()
    {
        using var workspace = new TestWorkspace();
        var project = CopyFixture(workspace);
        workspace.Write("Dynamic.cs", "using Microsoft.Extensions.Localization; public class Dynamic { public string Get(IStringLocalizer<Dynamic> words, string key) => words[key]; }");
        Assert.Contains("OCLOC001", await BuildAsync(project, Update));
        var cached = await BuildAsync(project, Update);
        Assert.Contains("up to date", cached);
        Assert.Contains("OCLOC001", cached);
        Assert.DoesNotContain("OCLOC001", await BuildAsync(project));
        var strict = await RunDotnetAsync("build", project, "--tl:off", "--nologo", "-v:minimal", Update, "-p:LocalizationStrict=true");
        Assert.NotEqual(0, strict.ExitCode);
        Assert.Contains("OCLOC001", strict.Output);
    }

    [Fact]
    public async Task Update_SeparateProjectsInParallel_ProducesIndependentTemplates()
    {
        using var first = new TestWorkspace();
        using var second = new TestWorkspace();
        var projects = new[] { CopyFixture(first), CopyFixture(second) };
        await Task.WhenAll(projects.Select(project => BuildAsync(project, Update)));
        Assert.Equal(await File.ReadAllTextAsync(Path.Combine(first.DirectoryPath, "RazorModule.pot"), TestContext.Current.CancellationToken), await File.ReadAllTextAsync(Path.Combine(second.DirectoryPath, "RazorModule.pot"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Pack_MultiTargetConsumer_UpdatesOneTemplateAndEmbedsItWithoutRuntimeTooling()
    {
        using var workspace = new TestWorkspace();
        var packageProject = Path.Combine(TestWorkspace.RepositoryRoot, "src/OrchardCore/OrchardCore.Localization.Build/OrchardCore.Localization.Build.csproj");
        var feed = Path.Combine(workspace.DirectoryPath, "feed");
        const string version = "0.0.0-localizationtests";
        await RequireSuccessAsync("pack", packageProject, "--tl:off", "--nologo", "-c", "Debug", "-o", feed, "-p:PackageVersion=" + version, "-v:quiet");
        using (var archive = ZipFile.OpenRead(Directory.GetFiles(feed, "*.nupkg").Single()))
        {
            Assert.Contains(archive.Entries, entry => entry.FullName == "build/OrchardCore.Localization.Build.targets");
            Assert.Contains(archive.Entries, entry => entry.FullName == "buildMultiTargeting/OrchardCore.Localization.Build.targets");
            Assert.Contains(archive.Entries, entry => entry.FullName == "tools/any/OrchardCore.Localization.Tools.dll");
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
        var template = Path.Combine(workspace.DirectoryPath, "Consumer/Consumer.pot");

        // The target of a multi-targeting project updates the template from its first target framework.
        var output = await RequireSuccessAsync("msbuild", consumer, "-restore", "-t:UpdateLocalizationCatalog", "--tl:off", "--nologo", "-v:minimal");
        Assert.Equal(1, output.Split("messages in Consumer.pot").Length - 1);
        var text = await File.ReadAllTextAsync(template, TestContext.Current.CancellationToken);
        Assert.Contains("Packaged message", text);
        Assert.Contains("up to date", await BuildAsync(consumer, Update));

        await BuildAsync(consumer);
        foreach (var framework in new[] { "netstandard2.0", "net10.0" })
        {
            var assembly = Path.Combine(workspace.DirectoryPath, "Consumer/bin/Debug", framework, "Consumer.dll");
            Assert.Equal(text, ReadResource(assembly, "Consumer.Localization.pot"));
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
        => BuildAsync(project, []);

    private static Task<string> BuildAsync(string project, params string[] properties)
        => RequireSuccessAsync(["build", project, "--tl:off", "--nologo", "-v:minimal", .. properties]);

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

    private static bool HasResource(string path, string name)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();

        return metadata.ManifestResources.Select(metadata.GetManifestResource).Any(resource => metadata.GetString(resource.Name) == name);
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
