using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;

namespace OrchardCore.Cli;

internal sealed class DeviceLoginSessionStore
{
    private readonly string _directory;

    public DeviceLoginSessionStore(CliPaths paths)
    {
        _directory = Path.Combine(paths.RootDirectory, "device-logins");
    }

    public async Task SaveAsync(DeviceLoginSession session, CancellationToken cancellationToken)
    {
        var path = GetPath(session.SessionId);
        EnsureDirectory();
        var temporary = path + "." + Guid.NewGuid().ToString("n") + ".tmp";
        var payload = JsonSerializer.SerializeToUtf8Bytes(session, CliJsonContext.Default.DeviceLoginSession);
        try
        {
            await using (var file = new FileStream(temporary, Options(FileMode.CreateNew)))
            {
                await file.WriteAsync(payload, cancellationToken);
                await file.FlushAsync(cancellationToken);
            }
            await AtomicFile.ReplaceAsync(temporary, path, cancellationToken);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(payload);
            File.Delete(temporary);
        }
    }

    public async Task<DeviceLoginSession> ReadAsync(string sessionId, CancellationToken cancellationToken)
    {
        var path = GetPath(sessionId);
        try
        {
            RejectLink(_directory);
            RejectLink(path);
            await using var stream = AtomicFile.OpenRead(path);
            var session = await JsonSerializer.DeserializeAsync(stream, CliJsonContext.Default.DeviceLoginSession, cancellationToken);
            if (session is null || session.SessionId != sessionId || string.IsNullOrEmpty(session.DeviceCode)
                || session.IntervalSeconds <= 0)
            {
                throw new CliException("Invalid device login session. Run 'pomi login device start' again.");
            }
            return session;
        }
        catch (FileNotFoundException)
        {
            throw MissingSession();
        }
        catch (DirectoryNotFoundException)
        {
            throw MissingSession();
        }
    }

    public FileStream Acquire(string sessionId)
    {
        var path = GetPath(sessionId) + ".lock";
        EnsureDirectory();
        RejectLink(path);
        try
        {
            // Keep the lock file: unlinking it could let another process lock a
            // different inode while a waiter still owns the original lock.
            return new FileStream(path, Options(FileMode.OpenOrCreate));
        }
        catch (IOException)
        {
            throw new CliException("Another process is already waiting for this device login session.");
        }
    }

    public void Delete(string sessionId) => File.Delete(GetPath(sessionId));

    public async Task PruneExpiredAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_directory))
        {
            return;
        }
        foreach (var path in Directory.EnumerateFiles(_directory, "*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = Path.GetFileNameWithoutExtension(path);
            try
            {
                using var lease = Acquire(id);
                var session = await ReadAsync(id, cancellationToken);
                if (session.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    Delete(id);
                }
            }
            catch (Exception exception) when (exception is CliException or JsonException or IOException)
            {
                // Leave active or unreadable sessions alone; starting another
                // login must not change their state.
            }
        }
    }

    private string GetPath(string sessionId)
    {
        if (!Guid.TryParseExact(sessionId, "N", out var id) || id.ToString("n") != sessionId)
        {
            throw new CliException("Invalid device login session ID. Use the sessionId returned by 'pomi login device start', not the user code.");
        }
        return Path.Combine(_directory, sessionId + ".json");
    }

    private void EnsureDirectory()
    {
        RejectLink(_directory);
        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(_directory);
            var user = WindowsIdentity.GetCurrent().User ?? throw new CliException("Unable to determine the current Windows user.");
            var security = new DirectorySecurity();
            security.SetOwner(user);
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            security.AddAccessRule(new FileSystemAccessRule(user, FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            new DirectoryInfo(_directory).SetAccessControl(security);
        }
        else
        {
            Directory.CreateDirectory(_directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            CliPaths.SetOwnerOnlyDirectory(_directory);
        }
    }

    private static FileStreamOptions Options(FileMode mode)
    {
        var options = new FileStreamOptions { Mode = mode, Access = FileAccess.ReadWrite, Share = FileShare.None, Options = FileOptions.Asynchronous };
        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }
        return options;
    }

    private static void RejectLink(string path)
    {
        if (new FileInfo(path).LinkTarget is not null || new DirectoryInfo(path).LinkTarget is not null)
        {
            throw new CliException("Device login session paths must not be symbolic links.");
        }
    }

    private static CliException MissingSession() => new("Device login session not found or already completed. Run 'pomi login device start' to create a new one.");
}
