using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace OrchardCore.Cli;

internal interface ICredentialStore
{
    string DisplayName { get; }

    Task SaveAsync(string contextName, StoredToken token, CancellationToken cancellationToken);

    Task<StoredToken?> GetAsync(string contextName, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(string contextName, CancellationToken cancellationToken);
}

internal static class CredentialStoreFactory
{
    public static ICredentialStore CreateDefault(CliPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        if (OperatingSystem.IsWindows())
        {
            return new WindowsCredentialStore();
        }

        return new FileCredentialStore(paths);
    }
}

internal sealed class FileCredentialStore : ICredentialStore
{
    private readonly CliPaths _paths;

    public FileCredentialStore(CliPaths paths)
    {
        _paths = paths;
    }

    public string DisplayName => "plaintext-file";

    public async Task SaveAsync(string contextName, StoredToken token, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        ArgumentNullException.ThrowIfNull(token);

        var path = _paths.GetCredentialFilePath(contextName);
        var temporaryPath = $"{path}.{Guid.NewGuid():n}.tmp";
        try
        {
            var options = new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                BufferSize = 4096,
                Options = FileOptions.Asynchronous | FileOptions.WriteThrough,
            };
            if (!OperatingSystem.IsWindows())
            {
                options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
            }

            var payload = Encoding.UTF8.GetBytes(CliUtilities.SerializeToken(token));
            try
            {
                await using var stream = new FileStream(temporaryPath, options);
                await stream.WriteAsync(payload, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(payload);
            }

            await AtomicFile.ReplaceAsync(temporaryPath, path, cancellationToken);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    public async Task<StoredToken?> GetAsync(string contextName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);

        var path = _paths.GetCredentialFilePath(contextName);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return CliUtilities.DeserializeToken(await File.ReadAllTextAsync(path, cancellationToken));
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    public Task<bool> DeleteAsync(string contextName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        cancellationToken.ThrowIfCancellationRequested();

        var path = _paths.GetCredentialFilePath(contextName);
        if (!File.Exists(path))
        {
            return Task.FromResult(false);
        }

        File.Delete(path);
        return Task.FromResult(true);
    }
}

internal sealed class WindowsCredentialStore : ICredentialStore
{
    private const uint GenericCredential = 1;
    private const uint LocalMachinePersistence = 2;
    private const int NotFoundError = 1168;

    public string DisplayName => "windows-credential-manager";

    public Task SaveAsync(string contextName, StoredToken token, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        ArgumentNullException.ThrowIfNull(token);
        cancellationToken.ThrowIfCancellationRequested();

        var payload = Encoding.UTF8.GetBytes(CliUtilities.SerializeToken(token));
        if (payload.Length > 2560)
        {
            throw new CliException("The token is too large for Windows Credential Manager.");
        }

        var blob = Marshal.AllocHGlobal(payload.Length);
        try
        {
            Marshal.Copy(payload, 0, blob, payload.Length);
            var credential = new NativeCredential
            {
                Type = GenericCredential,
                TargetName = Marshal.StringToCoTaskMemUni(GetTargetName(contextName)),
                CredentialBlobSize = checked((uint)payload.Length),
                CredentialBlob = blob,
                Persist = LocalMachinePersistence,
                UserName = Marshal.StringToCoTaskMemUni(contextName),
            };

            try
            {
                if (!CredWrite(ref credential, 0))
                {
                    throw CreateWindowsException("save");
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(credential.TargetName);
                Marshal.FreeCoTaskMem(credential.UserName);
            }
        }
        finally
        {
            CryptographicOperations.ZeroMemory(payload);
            Marshal.FreeHGlobal(blob);
        }

        return Task.CompletedTask;
    }

    public Task<StoredToken?> GetAsync(string contextName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        cancellationToken.ThrowIfCancellationRequested();

        if (!CredRead(GetTargetName(contextName), GenericCredential, 0, out var credentialPointer))
        {
            if (Marshal.GetLastPInvokeError() == NotFoundError)
            {
                return Task.FromResult<StoredToken?>(null);
            }

            throw CreateWindowsException("read");
        }

        try
        {
            var credential = Marshal.PtrToStructure<NativeCredential>(credentialPointer);
            var payload = new byte[checked((int)credential.CredentialBlobSize)];
            Marshal.Copy(credential.CredentialBlob, payload, 0, payload.Length);
            var serialized = Encoding.UTF8.GetString(payload);
            CryptographicOperations.ZeroMemory(payload);
            return Task.FromResult<StoredToken?>(CliUtilities.DeserializeToken(serialized));
        }
        finally
        {
            CredFree(credentialPointer);
        }
    }

    public Task<bool> DeleteAsync(string contextName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        cancellationToken.ThrowIfCancellationRequested();

        if (CredDelete(GetTargetName(contextName), GenericCredential, 0))
        {
            return Task.FromResult(true);
        }

        if (Marshal.GetLastPInvokeError() == NotFoundError)
        {
            return Task.FromResult(false);
        }

        throw CreateWindowsException("delete");
    }

    private static CliException CreateWindowsException(string operation) =>
        new($"The Windows Credential Manager {operation} operation failed with error {Marshal.GetLastPInvokeError()}.");

    private static string GetTargetName(string contextName) => $"OrchardCore.oc:{contextName}";

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCredential
    {
        public uint Flags;
        public uint Type;
        public IntPtr TargetName;
        public IntPtr Comment;
        public long LastWritten;
        public uint CredentialBlobSize;
        public IntPtr CredentialBlob;
        public uint Persist;
        public uint AttributeCount;
        public IntPtr Attributes;
        public IntPtr TargetAlias;
        public IntPtr UserName;
    }

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredWrite(ref NativeCredential credential, uint flags);

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredRead(string target, uint type, uint flags, out IntPtr credential);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredDelete(string target, uint type, uint flags);

    [DllImport("advapi32.dll")]
    private static extern void CredFree(IntPtr buffer);
}
