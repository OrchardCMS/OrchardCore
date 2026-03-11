using tusdotnet.Interfaces;

namespace OrchardCore.Media.Services;

/// <summary>
/// A no-op TUS file lock provider that never blocks.
/// Used because our <see cref="MediaTusStore"/> handles offsets via sidecar files
/// and does not require external locking. This avoids HTTP 423 (Locked) errors
/// when a client pauses and resumes an upload before the aborted PATCH request
/// has fully completed on the server.
/// </summary>
internal sealed class NoOpFileLockProvider : ITusFileLockProvider
{
    public Task<ITusFileLock> AquireLock(string fileId)
    {
        return Task.FromResult<ITusFileLock>(new NoOpFileLock());
    }

    private sealed class NoOpFileLock : ITusFileLock
    {
        public Task<bool> Lock()
        {
            return Task.FromResult(true);
        }

        public Task ReleaseIfHeld()
        {
            return Task.CompletedTask;
        }
    }
}
