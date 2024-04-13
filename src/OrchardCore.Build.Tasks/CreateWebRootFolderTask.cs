namespace OrchardCore.Build.Tasks;

public class CreateWebRootFolderTask : CreateWebFolderTask
{
    public override string FolderName => "wwwroot";
}
