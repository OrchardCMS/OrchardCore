using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Media.Services;

/// <summary>
/// Necessary because of IStringLocalizer of T
/// </summary>
public class MediaJSLocalizer(IStringLocalizer<MediaJSLocalizer> S) : IJSLocalizer
{
    /// <summary>
    /// This dictionary needs to be affected either here or
    /// in a .cshtml template for the po Extractor to find the strings to translate.
    /// </summary>
    /// <returns>Returns a list of localized strings</returns>
    public Dictionary<string, string> GetLocalizations(string[] groups)
    {
        if (groups.Contains("media-app", StringComparer.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                // Localizable Strings shared
                { "NewFolder", S["New folder"].Value },
                { "DeleteFolderTitle", S["Delete folder"].Value },
                { "DeleteFolderMessage", S["Are you sure you want to delete this folder?"].Value },
                { "DeleteFileTitle", S["Delete file"].Value },
                { "DeleteFileMessage", S["Are you sure you want to delete this/these file(s)?"].Value },
                { "RenameFileTitle", S["Rename file"].Value },
                { "RenameFileMessage", S["New name"].Value },
                { "MoveFileTitle", S["Move file(s)"].Value },
                { "MoveFileMessage", S["Are you sure you want to move the selected file(s) to this folder?"].Value },
                { "CreateFolderTitle", S["Create folder"].Value },
                { "CreateFolderMessage", S["Folder name"].Value },
                { "SameFolderMessage", S["The file is already in this folder"].Value },
                { "EditButton", S["Rename"].Value },
                { "DeleteButton", S["Delete"].Value },
                { "DownloadButton", S["Download"].Value },
                { "SelectAll", S["Select All"].Value },
                { "FolderEmpty", S["This folder is empty"].Value },
                { "FolderFilterEmpty", S["Nothing to show with this filter"].Value },
                { "Upload", S["Upload"].Value },
                { "UploadFiles", S["Upload files"].Value },
                { "SelectNone", S["Select None"].Value },
                { "Invert", S["Invert"].Value },
                { "Delete", S["Delete"].Value },
                { "DropHere", S["Drop your file here"].Value },
                { "DropTitle", S["Your files will be uploaded to the current folder when you drop them here"].Value },
                { "FileLibrary", S["Files"].Value },
                { "FolderRoot", S["/"].Value },
                { "Filter", S["Search and filter..."].Value },
                { "Cancel", S["Cancel"].Value },
                // Localizable Strings for uploadComponent
                { "Error", S["This file exceeds the maximum upload size"].Value },
                // Localizable Strings for uploadListcomponent
                { "Uploads", S["Uploads"].Value },
                { "Errors", S["Errors"].Value },
                { "ClearErrors", S["Clear Errors"].Value },
                // Localizable Strings for fileItemsTableComponent
                { "ImageHeader", S["File"].Value },
                { "NameHeader", S["Name"].Value },
                { "LastModifyHeader", S["Last modified"].Value },
                { "SizeHeader", S["Size"].Value },
                { "TypeHeader", S["Type"].Value },
                // Localizable Strings for pagerComponent
                { "PagerFirstButton", S["First"].Value },
                { "PagerPreviousButton", S["Previous"].Value },
                { "PagerNextButton", S["Next"].Value },
                { "PagerLastButton", S["Last"].Value },
                { "PagerPageSizeLabel", S["Page Size"].Value },
                { "PagerPageLabel", S["Page:"].Value },
                { "PagerTotalLabel", S["Total items:"].Value },
                // Error messages
                { "ErrorRenamingFile", S["Error renaming file"].Value },
                { "ErrorMovingFile", S["Error moving file"].Value },
                { "ErrorDeleteFile", S["Error deleting file"].Value },
                { "ErrorDeleteFiles", S["Error deleting files"].Value },
                { "ErrorCreateFolder", S["Error creating folder"].Value },
                { "ErrorDeleteFolder", S["Error deleting folder"].Value },
                { "ErrorGetFolders", S["Error getting folders"].Value },
                { "Ok", S["Ok"].Value },
                { "ActionFileTitle", S["File Action"].Value },
                { "ActionFileMessage", S["What do you want to do with this file?"].Value },
                { "BulkActionFilesTitle", S["Files Bulk Action"].Value },
                { "BulkActionFilesMessage", S["What do you want to do with these file(s)?"].Value },
                { "Rename", S["Rename"].Value },
                { "Download", S["Download"].Value },
                { "Processing", S["Processing"].Value },
                { "Common", S["Common"].Value },
                { "Status", S["Status"].Value },
                { "RenamingFileWarning", S["Renaming file failed. File name was identical."].Value },
                { "CreateSubFolder", S["Create a subfolder"].Value },
                { "ActionFolderTitle", S["Folder Action"].Value },
                { "ActionFolderMessage", S["What do you want to do with this folder?"].Value },
            };
        }

        return null;
    }
}
