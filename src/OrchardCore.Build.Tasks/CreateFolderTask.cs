using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace OrchardCore.Build.Tasks;

public abstract class CreateWebFolderTask : MSBuildTask
{
    public abstract string FolderName { get; }

    public override bool Execute()
    {
        if (!Directory.Exists(FolderName))
        {
            Directory.CreateDirectory(FolderName);

            var dummyFilePath = Path.Combine(FolderName, ".placeholder");

            if (!File.Exists(dummyFilePath))
            {
                File.WriteAllText(dummyFilePath, string.Empty);
            }
        }

        return true;
    }
}
