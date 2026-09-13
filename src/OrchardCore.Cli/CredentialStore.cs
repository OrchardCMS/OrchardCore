using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace OrchardCore.Cli;

internal interface ICredentialStore
{
    string DisplayName { get; }

    bool SupportsPersistentHumanTokens { get; }

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

    public bool SupportsPersistentHumanTokens => true;

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

internal sealed class UnsupportedCredentialStore : ICredentialStore
{
    public string DisplayName => "unsupported";

    public bool SupportsPersistentHumanTokens => false;

    public Task SaveAsync(string contextName, StoredToken token, CancellationToken cancellationToken) =>
        throw new CliException("Persistent human credentials require Windows Credential Manager, macOS Keychain, or Linux Secret Service. Provide client credentials per command when no supported secure store is available.");

    public Task<StoredToken?> GetAsync(string contextName, CancellationToken cancellationToken) => Task.FromResult<StoredToken?>(null);

    public Task<bool> DeleteAsync(string contextName, CancellationToken cancellationToken) => Task.FromResult(false);
}

internal sealed class MacOsCredentialStore : ICredentialStore
{
    // Keep the credential service identifier stable across the executable rename.
    private const string ServiceName = "OrchardCore.oc";
    private const int ItemNotFound = -25300;
    private const string SecurityFramework = "/System/Library/Frameworks/Security.framework/Security";
    private const string CoreFoundationFramework = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    public string DisplayName => "macos-keychain";

    public bool SupportsPersistentHumanTokens => true;

    public Task SaveAsync(string contextName, StoredToken token, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        ArgumentNullException.ThrowIfNull(token);
        cancellationToken.ThrowIfCancellationRequested();

        _ = Delete(contextName);

        var service = Encoding.UTF8.GetBytes(ServiceName);
        var account = Encoding.UTF8.GetBytes(contextName);
        var payload = CliUtilities.SerializeToken(token);
        var password = Encoding.UTF8.GetBytes(payload);
        try
        {
            var status = SecKeychainAddGenericPassword(
                IntPtr.Zero,
                checked((uint)service.Length),
                service,
                checked((uint)account.Length),
                account,
                checked((uint)password.Length),
                password,
                out var item);

            Release(item);
            ThrowIfFailed(status, "save");
            return Task.CompletedTask;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(password);
        }
    }

    public Task<StoredToken?> GetAsync(string contextName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        cancellationToken.ThrowIfCancellationRequested();

        var service = Encoding.UTF8.GetBytes(ServiceName);
        var account = Encoding.UTF8.GetBytes(contextName);
        var status = SecKeychainFindGenericPassword(
            IntPtr.Zero,
            checked((uint)service.Length),
            service,
            checked((uint)account.Length),
            account,
            out var passwordLength,
            out var passwordData,
            out var item);

        if (status == ItemNotFound)
        {
            return Task.FromResult<StoredToken?>(null);
        }

        try
        {
            ThrowIfFailed(status, "read");
            var password = new byte[checked((int)passwordLength)];
            Marshal.Copy(passwordData, password, 0, password.Length);
            var payload = Encoding.UTF8.GetString(password);
            CryptographicOperations.ZeroMemory(password);
            return Task.FromResult<StoredToken?>(CliUtilities.DeserializeToken(payload));
        }
        finally
        {
            if (passwordData != IntPtr.Zero)
            {
                _ = SecKeychainItemFreeContent(IntPtr.Zero, passwordData);
            }

            Release(item);
        }
    }

    public Task<bool> DeleteAsync(string contextName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Delete(contextName));
    }

    private static bool Delete(string contextName)
    {
        var service = Encoding.UTF8.GetBytes(ServiceName);
        var account = Encoding.UTF8.GetBytes(contextName);
        var status = SecKeychainFindGenericPassword(
            IntPtr.Zero,
            checked((uint)service.Length),
            service,
            checked((uint)account.Length),
            account,
            out _,
            out var passwordData,
            out var item);

        if (passwordData != IntPtr.Zero)
        {
            _ = SecKeychainItemFreeContent(IntPtr.Zero, passwordData);
        }

        if (status == ItemNotFound)
        {
            Release(item);
            return false;
        }

        ThrowIfFailed(status, "find");
        status = SecKeychainItemDelete(item);
        Release(item);
        ThrowIfFailed(status, "delete");
        return true;
    }

    private static void ThrowIfFailed(int status, string operation)
    {
        if (status != 0)
        {
            throw new CliException($"The macOS Keychain {operation} operation failed with status {status}.");
        }
    }

    private static void Release(IntPtr item)
    {
        if (item != IntPtr.Zero)
        {
            CFRelease(item);
        }
    }

    [DllImport(SecurityFramework)]
    private static extern int SecKeychainAddGenericPassword(
        IntPtr keychain,
        uint serviceNameLength,
        byte[] serviceName,
        uint accountNameLength,
        byte[] accountName,
        uint passwordLength,
        byte[] passwordData,
        out IntPtr item);

    [DllImport(SecurityFramework)]
    private static extern int SecKeychainFindGenericPassword(
        IntPtr keychainOrArray,
        uint serviceNameLength,
        byte[] serviceName,
        uint accountNameLength,
        byte[] accountName,
        out uint passwordLength,
        out IntPtr passwordData,
        out IntPtr item);

    [DllImport(SecurityFramework)]
    private static extern int SecKeychainItemDelete(IntPtr item);

    [DllImport(SecurityFramework)]
    private static extern int SecKeychainItemFreeContent(IntPtr attributeList, IntPtr data);

    [DllImport(CoreFoundationFramework)]
    private static extern void CFRelease(IntPtr value);
}

internal sealed class WindowsCredentialStore : ICredentialStore
{
    private const uint GenericCredential = 1;
    private const uint LocalMachinePersistence = 2;
    private const int NotFoundError = 1168;

    public string DisplayName => "windows-credential-manager";

    public bool SupportsPersistentHumanTokens => true;

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

internal sealed class LinuxSecretServiceCredentialStore : ICredentialStore
{
    // Keep the credential service identifier stable across the executable rename.
    private const string ServiceName = "OrchardCore.oc";

    public string DisplayName => "linux-secret-service";

    public bool SupportsPersistentHumanTokens => true;

    public async Task SaveAsync(string contextName, StoredToken token, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        ArgumentNullException.ThrowIfNull(token);

        var payload = CliUtilities.SerializeToken(token);
        _ = await RunSecretToolAsync(
            ["store", "--label=Pomi CLI", "service", ServiceName, "context", contextName],
            payload,
            allowNotFound: false,
            cancellationToken);
    }

    public async Task<StoredToken?> GetAsync(string contextName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        var payload = await RunSecretToolAsync(
            ["lookup", "service", ServiceName, "context", contextName],
            standardInput: null,
            allowNotFound: true,
            cancellationToken);

        return string.IsNullOrWhiteSpace(payload) ? null : CliUtilities.DeserializeToken(payload.Trim());
    }

    public async Task<bool> DeleteAsync(string contextName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextName);
        var result = await RunSecretToolAsync(
            ["clear", "service", ServiceName, "context", contextName],
            standardInput: null,
            allowNotFound: true,
            cancellationToken);

        return result is not null;
    }

    private static async Task<string?> RunSecretToolAsync(
        IEnumerable<string> arguments,
        string? standardInput,
        bool allowNotFound,
        CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "secret-tool",
                RedirectStandardInput = standardInput is not null,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        try
        {
            process.Start();
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or FileNotFoundException)
        {
            throw new CliException("Linux Secret Service requires the 'secret-tool' executable and an available Secret Service provider.");
        }

        if (standardInput is not null)
        {
            await process.StandardInput.WriteAsync(standardInput.AsMemory(), cancellationToken);
            process.StandardInput.Close();
        }

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            if (allowNotFound)
            {
                return null;
            }

            throw new CliException(string.IsNullOrWhiteSpace(error)
                ? "The Linux Secret Service operation failed."
                : error.Trim());
        }

        return output;
    }
}
