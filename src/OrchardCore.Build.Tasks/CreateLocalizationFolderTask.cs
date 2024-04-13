using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace OrchardCore.Build.Tasks;

public class CreateLocalizationFolderTask : MSBuildTask
{
    private const string LocalizationFolderName = "Localization";

    public override bool Execute()
    {
        if (!File.Exists(LocalizationFolderName))
        {
            Directory.CreateDirectory(LocalizationFolderName);

            File.WriteAllText(Path.Combine(LocalizationFolderName, ".placeholder"), string.Empty);
        }

        return true;
    }
}
