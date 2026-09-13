namespace OrchardCore.Cli.Tests;

internal static class TestPaths
{
    public static string CreateScratchDirectory(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestScratch", name, Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(path);
        return path;
    }
}
