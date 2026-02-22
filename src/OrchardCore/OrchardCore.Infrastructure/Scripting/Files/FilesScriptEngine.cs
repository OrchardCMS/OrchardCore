using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting.Files;

/// <summary>
/// Provides file-based scripting operations like [file:text('path')] and [file:base64('path')].
/// Requires a valid <see cref="IFileProvider"/> and base path to function.
/// </summary>
public class FilesScriptEngine : IScriptingEngine
{
    public string Prefix => "file";

    public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
    {
        // FileProvider and basePath are required for this engine.
        // If not provided, return a scope that will throw on evaluation.
        return new FilesScriptScope(fileProvider, basePath);
    }

    public object Evaluate(IScriptingScope scope, string script)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (scope is not FilesScriptScope fileScope)
        {
            throw new ArgumentException($"Expected a scope of type {nameof(FilesScriptScope)}", nameof(scope));
        }

        if (fileScope.FileProvider is null)
        {
            throw new InvalidOperationException("The file scripting engine requires a file provider. This recipe source does not support file operations.");
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
