using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Cli;

/// <summary>Reserves a private destination before requesting a one-time secret response.</summary>
internal sealed class SecretOutputFile : IAsyncDisposable
{
    private readonly FileStream _stream;
    private readonly string _path;
    private bool _hasResponse;

    private SecretOutputFile(string path, FileStream stream)
    {
        _path = path;
        _stream = stream;
    }

    public static SecretOutputFile Create(FileInfo destination)
    {
        var path = destination.FullName;
        try
        {
            FileStream stream;
            if (OperatingSystem.IsWindows())
            {
                var user = WindowsIdentity.GetCurrent().User ?? throw new CliException("Unable to determine the current Windows user.");
                var security = new FileSecurity();
                security.SetOwner(user);
                security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
                security.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl, AccessControlType.Allow));
                stream = new FileInfo(path).Create(FileMode.CreateNew, FileSystemRights.FullControl, FileShare.None,
                    4096, FileOptions.Asynchronous | FileOptions.WriteThrough, security);
            }
            else
            {
                stream = new FileStream(path, new FileStreamOptions
                {
                    Mode = FileMode.CreateNew,
                    Access = FileAccess.Write,
                    Share = FileShare.None,
                    Options = FileOptions.Asynchronous | FileOptions.WriteThrough,
                    UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite,
                });
            }
            return new SecretOutputFile(path, stream);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new CliException("Cannot reserve the secret output file. Choose a new file in an existing writable directory; existing files are never overwritten.");
        }
    }

    public async Task<JsonElement> WriteAsync(JsonElement response, CancellationToken cancellationToken)
    {
        if (response.ValueKind == JsonValueKind.Undefined)
        {
            throw new CliException("The server returned no JSON secret response.");
        }
        var payload = Encoding.UTF8.GetBytes(response.GetRawText());
        // Once a response has arrived, retain the private file even on a partial write.
        // The caller may be able to recover a credential that the server cannot repeat.
        _hasResponse = true;
        try
        {
            await _stream.WriteAsync(payload, cancellationToken);
            await _stream.FlushAsync(cancellationToken);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(payload);
        }
        // Do not project any server response fields to stdout, including unknown fields.
        return JsonSerializer.SerializeToElement(new JsonObject { ["secretOutputFile"] = _path }, CliJsonContext.Default.JsonObject);
    }

    public async Task<JsonElement> WriteStreamAsync(Stream response, CancellationToken cancellationToken)
    {
        await response.CopyToAsync(_stream, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
        _hasResponse = true;
        return JsonSerializer.SerializeToElement(new JsonObject { ["outputFile"] = _path, ["length"] = _stream.Length }, CliJsonContext.Default.JsonObject);
    }

    public async ValueTask DisposeAsync()
    {
        await _stream.DisposeAsync();
        if (!_hasResponse)
        {
            File.Delete(_path);
        }
    }
}
