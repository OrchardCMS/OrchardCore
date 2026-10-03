namespace OrchardCore.Localization.Extraction;

public sealed record ExtractionFile(string Path, string LogicalPath);

public sealed class ExtractionOptions
{
    public required string ProjectDirectory { get; init; }
    public required string AssemblyName { get; init; }
    public string RootNamespace { get; init; } = "";
    public string ViewPrefix { get; init; } = "";
    public string LanguageVersion { get; init; } = "14.0";
    public string InterceptorsNamespaces { get; init; } = "";
    public string RazorLanguageVersion { get; init; } = "10.0";
    public string RazorConfiguration { get; init; } = "MVC-3.0";
    public string Nullable { get; init; } = "disable";
    public string OutputType { get; init; } = "Library";
    public bool AllowUnsafe { get; init; }
    public bool Strict { get; init; }
    public IList<string> Defines { get; } = new List<string>();
    public IList<string> References { get; } = new List<string>();
    public IList<string> Analyzers { get; } = new List<string>();
    public IDictionary<string, string> ReferenceAliases { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
    public IList<ExtractionFile> Sources { get; } = new List<ExtractionFile>();
    public IList<ExtractionFile> RazorFiles { get; } = new List<ExtractionFile>();
    public IList<ExtractionFile> LiquidFiles { get; } = new List<ExtractionFile>();

    public string GetViewContext(string logicalPath)
    {
        var path = System.IO.Path.ChangeExtension(logicalPath.Replace('\\', '/'), null)!.TrimStart('/');
        if (path.StartsWith("Areas/", StringComparison.Ordinal))
        {
            path = path.Substring("Areas/".Length);
        }
        else if (ViewPrefix.Length > 0)
        {
            path = ViewPrefix + "/" + path;
        }

        return path.Replace('/', '.');
    }

    public SourceReference GetSource(string path, int line = 0)
    {
        var relative = System.IO.Path.IsPathRooted(path) ? System.IO.Path.GetRelativePath(ProjectDirectory, path) : path;
        return new SourceReference(relative.Replace('\\', '/'), line);
    }
}
