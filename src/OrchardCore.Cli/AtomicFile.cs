namespace OrchardCore.Cli;

internal static class AtomicFile
{
    public static FileStream OpenRead(string path) => new(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);

    public static async Task ReplaceAsync(string temporaryPath, string destinationPath, CancellationToken cancellationToken)
    {
        // Windows can briefly deny a concurrent replacement while another rename
        // or scanner holds the destination. Keep the completed temporary file and
        // retry that operation; never delete/truncate the destination first.
        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                File.Move(temporaryPath, destinationPath, overwrite: true);
                return;
            }
            catch (Exception exception) when (OperatingSystem.IsWindows()
                && attempt < 40
                && exception is IOException or UnauthorizedAccessException
                && (exception.HResult & 0xffff) is 5 or 32 or 33)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25), cancellationToken);
            }
        }
    }
}
