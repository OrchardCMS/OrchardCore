using System.Text.RegularExpressions;

namespace OrchardCore.Tests.Infrastructure;

public partial class CiWorkflowTests
{
    [Fact]
    public void IntegrationTests_ClassFilters_CoverEachClassExactlyOnce()
    {
        var root = GetRepositoryRoot();
        var commands = GetTestCommands(ReadWorkflow(root, "integration_tests.yml"));
        Assert.Equal(2, commands.Length);

        var testFiles = Directory.GetFiles(Path.Combine(root, "test", "OrchardCore.Tests.Integration"), "*Tests.cs", SearchOption.AllDirectories);
        Assert.NotEmpty(testFiles);

        foreach (var path in testFiles)
        {
            var source = File.ReadAllText(path);
            var className = $"{NamespaceRegex().Match(source).Groups[1].Value}.{Path.GetFileNameWithoutExtension(path)}";
            var matchingCommands = commands.Where(command => IncludesClass(command, className)).ToArray();

            Assert.True(matchingCommands.Length == 1, $"'{className}' must run in exactly one integration job.");

            var requiresExternalAzurite = source.Contains("[AzuriteFact]", StringComparison.Ordinal);
            Assert.Equal(requiresExternalAzurite, matchingCommands[0] == commands[1]);
        }
    }

    [Theory]
    [InlineData("pr_ci.yml")]
    [InlineData("main_ci.yml")]
    public void FunctionalTests_BaselineFilters_IncludeBulkSelectRegressionTests(string workflowName)
    {
        var root = GetRepositoryRoot();
        var workflow = ReadWorkflow(root, workflowName);
        var commands = GetTestCommands(workflow)
            .Where(command => command.Contains("OrchardCore.Tests.Functional.csproj", StringComparison.Ordinal))
            .ToArray();

        foreach (var testClass in new[] { "BulkSelectListTests", "TenantsBulkSelectTests" })
        {
            var source = File.ReadAllText(Path.Combine(root, "test", "OrchardCore.Tests.Functional", "Tests", "Cms", $"{testClass}.cs"));
            var className = $"{NamespaceRegex().Match(source).Groups[1].Value}.{testClass}";

            Assert.Single(commands, command => IncludesClass(command, className));
        }

        Assert.Matches("""(?s)- name: Functional Tests - CMS\s.*?env:\s+OrchardCore__YesSql__EnableThreadSafetyChecks: true\s+run:""", workflow);
        Assert.False(File.Exists(Path.Combine(root, ".github", "workflows", "functional_bulk_select_list.yml")));
    }

    private static bool IncludesClass(string command, string className)
    {
        var filters = ClassFilterRegex().Matches(command);
        var includes = filters.Where(filter => filter.Groups[1].Value != "-not").ToArray();
        var excludes = filters.Where(filter => filter.Groups[1].Value == "-not").ToArray();

        return (includes.Length == 0 || includes.Any(filter => MatchesClass(filter.Groups[2].Value, className)))
            && !excludes.Any(filter => MatchesClass(filter.Groups[2].Value, className));
    }

    private static bool MatchesClass(string filter, string className) =>
        Regex.IsMatch(className, "^" + Regex.Escape(filter).Replace("\\*", ".*") + "$", RegexOptions.CultureInvariant);

    private static string[] GetTestCommands(string workflow) =>
        workflow.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("dotnet test ", StringComparison.Ordinal))
            .ToArray();

    private static string ReadWorkflow(string root, string name) =>
        File.ReadAllText(Path.Combine(root, ".github", "workflows", name));

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OrchardCore.slnx")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            Assert.Skip("The repository sources are not available.");
        }

        return directory.FullName;
    }

    [GeneratedRegex(@"namespace ([\w.]+);")]
    private static partial Regex NamespaceRegex();

    [GeneratedRegex("--filter(-not)?-class \"([^\"]+)\"")]
    private static partial Regex ClassFilterRegex();
}
