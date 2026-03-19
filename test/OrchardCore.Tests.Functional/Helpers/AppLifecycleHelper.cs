namespace OrchardCore.Tests.Functional.Helpers;

public static class AppLifecycleHelper
{
    public static bool CopyRecipe(string appDir, string recipeFileName)
    {
        var destDir = Path.Combine(appDir, "Recipes");
        var destPath = Path.Combine(destDir, recipeFileName);

        if (File.Exists(destPath))
        {
            return false;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(recipeFileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            return false;
        }

        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var fileStream = File.Create(destPath);
        stream!.CopyTo(fileStream);

        return true;
    }

    public static void DeleteRecipe(string appDir, string recipeFileName)
    {
        var destDir = Path.Combine(appDir, "Recipes");
        var destPath = Path.Combine(destDir, recipeFileName);

        if (File.Exists(destPath))
        {
            File.Delete(destPath);
        }

        // Remove Recipes dir if empty.
        if (Directory.Exists(destDir) && !Directory.EnumerateFileSystemEntries(destDir).Any())
        {
            Directory.Delete(destDir);
        }
    }
}
