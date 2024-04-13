using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace OrchardCore.Build.Tasks;

public abstract class CreateWebFolderTask : MSBuildTask
{
    public abstract string FolderName { get; }

    public override bool Execute()
    {
        if (!File.Exists(FolderName))
        {
            Directory.CreateDirectory(FolderName);

            File.WriteAllText(Path.Combine(FolderName, ".placeholder"), string.Empty);
        }

        return true;
    }
}
