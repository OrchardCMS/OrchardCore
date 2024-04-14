using Microsoft.Build.Framework;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace OrchardCore.Build.Tasks;

public class CopyTranslationFilesTask : MSBuildTask
{
    public string SourceFile { get; set; }

    [Required]
    public string DestinationFolder { get; set; }

    public override bool Execute()
    {
        if (string.IsNullOrEmpty(SourceFile))
        {
            return true;
        }

        if (!Directory.Exists(DestinationFolder))
        {
            Directory.CreateDirectory(DestinationFolder);
        }

        var fileInfo = new FileInfo(SourceFile);

        fileInfo.CopyTo(Path.Combine(DestinationFolder, fileInfo.Name), overwrite: true);

        return true;
    }
}
