namespace OrchardCore.Tests.Utilities;

public static class TestFileHelper
{
    /// <summary>
    /// Deletes a file a test created, such as a temporary SQLite database, without failing the test when it cannot be removed.
    /// </summary>
    /// <remarks>
    /// On Windows another process, such as an antivirus scanner, can still hold a file that was just written.
    /// Removing it is only housekeeping, so a locked file must not fail the test.
    /// </remarks>
    /// <param name="path">The path of the file to delete. Nothing is done when it is <see langword="null"/> or the file does not exist.</param>
    public static void TryDelete(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
