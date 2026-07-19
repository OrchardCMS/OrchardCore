using System;
using System.Collections.Generic;

namespace OrchardCore.Media.ViewModels;

public class FileStoreEntryDto
{
    public string Name { get; set; }
    public long Size { get; set; }
    public string DirectoryPath { get; set; }
    public string FilePath { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public bool IsDirectory { get; set; }
    public string Url { get; set; }
    public string Mime { get; set; }
    public bool? HasChildren { get; set; }
}

public class PermittedStorageDto
{
    public long? Bytes { get; set; }
    public string Text { get; set; }
}

public class PaginatedFoldersDto
{
    public List<FileStoreEntryDto> Items { get; set; }
    public bool HasMore { get; set; }
}

public class DirectoryContentDto
{
    public List<FileStoreEntryDto> Folders { get; set; }
    public List<FileStoreEntryDto> Files { get; set; }
}

public class DirectoryTreeNodeDto
{
    public string Name { get; set; }
    public string Path { get; set; }
    public bool HasChildren { get; set; }
    public List<DirectoryTreeNodeDto> Children { get; set; }
}

public class MoveMedias
{
    public string[] mediaNames { get; set; }
    public string sourceFolder { get; set; }
    public string targetFolder { get; set; }
}
