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
                // App layout
                { "DropHere", S["Drop your file here"].Value },
                { "DropTitle", S["Your files will be uploaded to the current folder when you drop them here"].Value },
                { "UploadFiles", S["Upload files"].Value },
                { "DeleteAll", S["Delete"].Value },
                { "DownloadAll", S["Download"].Value },
                { "Filter", S["Search and filter..."].Value },
                { "FileLibrary", S["Files"].Value },
                { "FolderEmpty", S["This folder is empty"].Value },
                { "FolderFilterEmpty", S["Nothing to show with this filter"].Value },
                // File items table
                { "SelectAll", S["Select All"].Value },
                { "SelectNone", S["Select None"].Value },
                { "NameHeader", S["Name"].Value },
                { "LastModifyHeader", S["Last modified"].Value },
                { "SizeHeader", S["Size"].Value },
                // File context menu
                { "Rename", S["Rename"].Value },
                { "RenameSingleFileTitle", S["Rename file"].Value },
                { "Move", S["Move"].Value },
                { "MoveSingleFileTitle", S["Move file"].Value },
                { "Copy", S["Copy"].Value },
                { "CopySingleFileTitle", S["Copy file"].Value },
                { "Download", S["Download"].Value },
                { "Delete", S["Delete"].Value },
                { "DeleteFileTitle", S["Delete file"].Value },
                { "DeleteSingleFileMessage", S["Are you sure you want to delete this file?"].Value },
                { "DeleteMultipleFilesMessage", S["Are you sure you want to delete these files?"].Value },
                // File action modal
                { "Filename", S["File name"].Value },
                { "SelectFolder", S["Select a folder"].Value },
                { "SelectFile", S["Select a file"].Value },
                // Move confirmation modal
                { "MoveFileTitle", S["Move file(s)"].Value },
                { "MoveFileMessage", S["Are you sure you want to move the selected file(s) to this folder?"].Value },
                { "SameFolderMessage", S["The file is already in this folder"].Value },
                // Folder action modal
                { "ActionFolderTitle", S["Folder Action"].Value },
                { "ActionFolderMessage", S["What do you want to do with this folder?"].Value },
                { "CreateSubFolder", S["Create a subfolder"].Value },
                // Common modal buttons
                { "Ok", S["Ok"].Value },
                { "Cancel", S["Cancel"].Value },
                // Upload component
                { "SizeError", S["Error:"].Value },
                { "Uploads", S["Uploads"].Value },
                { "Errors", S["Errors"].Value },
                { "ClearErrors", S["Clear Errors"].Value },
                { "UploadResumed", S["Resumed"].Value },
                { "PauseUpload", S["Pause upload"].Value },
                { "ResumeUpload", S["Resume upload"].Value },
                { "CopyError", S["Copy error"].Value },
                { "Copied", S["Copied!"].Value },
                // Pager component
                { "PagerFirstButton", S["First"].Value },
                { "PagerPreviousButton", S["Previous"].Value },
                { "PagerNextButton", S["Next"].Value },
                { "PagerLastButton", S["Last"].Value },
                // Error messages
                { "Error", S["This file exceeds the maximum upload size"].Value },
                { "ErrorMovingFile", S["Error moving file"].Value },
                { "ErrorGetFolders", S["Error getting folders"].Value },
                { "ErrorDeleteRootFolder", S["Cannot delete the root folder"].Value },
                { "FailedDownload", S["Failed to download file"].Value },
                // Authorization messages
                { "Unauthorized", S["Unauthorized"].Value },
                { "UnauthorizedFile", S["You are not authorized to perform this action on this file"].Value },
                { "UnauthorizedFiles", S["You are not authorized to perform this action on these files"].Value },
                { "UnauthorizedFolder", S["You are not authorized to perform this action on this folder"].Value },
                // Success messages
                { "Success", S["Success"].Value },
                { "FilesMoved", S["File(s) moved successfully."].Value },
                { "FileCopied", S["File copied successfully."].Value },
                // Validation messages
                { "ValidationError", S["Validation error"].Value },
                { "ValidationErrorUploadFileExist", S["A file with this name already exists in the current folder."].Value },
                { "ValidationFileExtensionRequired", S["Invalid file extension"].Value },
                { "ValidationFilenameRequired", S["File name is required"].Value },
                { "ValidationFolderRequired", S["A folder must be selected"].Value },
            };
        }

        if (groups.Contains("media-field", StringComparer.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "noImages", S["No Files"].Value },
                { "addMedia", S["Add media"].Value },
                { "removeMedia", S["Remove"].Value },
                { "mediaText", S["Media text"].Value },
                { "editMediaText", S["Enter media text"].Value },
                { "anchor", S["Anchor"].Value },
                { "editAnchor", S["Select an anchor"].Value },
                { "resetAnchor", S["Reset Anchor"].Value },
                { "ok", S["OK"].Value },
                { "cancel", S["Cancel"].Value },
                { "mediaNotFound", S["Not Found"].Value },
                { "mediaTemporarilyUnavailable", S["Temporarily unavailable"].Value },
                { "smallThumbsTitle", S["Small Thumbs"].Value },
                { "largeThumbsTitle", S["Large Thumbs"].Value },
                { "dropFiles", S["Drop files here"].Value },
                { "uploads", S["Uploads"].Value },
                { "errors", S["Errors"].Value },
                { "clearErrors", S["Clear Errors"].Value },
                { "selectMedia", S["Select Media"].Value },
                { "loadingMediaBrowser", S["Loading media browser..."].Value },
            };
        }

        return null;
    }
}
