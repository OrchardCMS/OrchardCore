using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting.Files;

/// <summary>
/// Provides.
/// </summary>
public class FilesScriptEngine : IScriptingEngine
{
    public string Prefix => "file";

    public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
    {
        return new FilesScriptScope(fileProvider, basePath);
    }

    public object Evaluate(IScriptingScope scope, string script)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (scope is not FilesScriptScope fileScope)
        {
            throw new ArgumentException($"Expected a scope of type {nameof(FilesScriptScope)}", nameof(scope));
        }

        if (script.StartsWith("text('", StringComparison.Ordinal) && script.EndsWith("')", StringComparison.Ordinal))
        {
            var filePath = script[6..^2];
            var fileInfo = fileScope.FileProvider.GetRelativeFileInfo(fileScope.BasePath, filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(filePath);
            }

            using var fileStream = fileInfo.CreateReadStream();
            using var streamReader = new StreamReader(fileStream);
            return streamReader.ReadToEnd();
        }
        else if (script.StartsWith("base64('", StringComparison.Ordinal) && script.EndsWith("')", StringComparison.Ordinal))
        {
            var filePath = script[8..^2];
            var fileInfo = fileScope.FileProvider.GetRelativeFileInfo(fileScope.BasePath, filePath);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(filePath);
            }

            using var fileStream = fileInfo.CreateReadStream();
            using var memoryStream = MemoryStreamFactory.GetStream();
            memoryStream.WriteTo(fileStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }
        else
        {
            throw new ArgumentException($"Unknown command '{script}'");
        }
    }
}
