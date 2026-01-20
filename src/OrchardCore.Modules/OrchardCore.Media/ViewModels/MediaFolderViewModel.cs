namespace OrchardCore.Media.ViewModels;

public class MediaFolderViewModel
{
    public string Path { get; set; }

    public string Name { get; set; }

    public string DirectoryPath { get; set; }

    public long Length { get; set; }

    public DateTime LastModifiedUtc { get; set; }

    public bool IsDirectory { get; set; }

    public bool CanCreateFolder { get; set; }

    public bool CanDeleteFolder { get; set; }
}
