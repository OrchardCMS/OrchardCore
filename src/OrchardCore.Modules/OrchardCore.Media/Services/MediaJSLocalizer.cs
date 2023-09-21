using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Media.Services;


/// <summary>
/// Necessary because of IStringLocalizer of T
/// </summary>
public class MediaJSLocalizer: IJSLocalizer
{
    private readonly IStringLocalizer S;

    public MediaJSLocalizer(IStringLocalizer<MediaJSLocalizer> localizer)
    {
        S = localizer;
    }

    /// <summary>
    /// This dictionary needs to be affected either here or
    /// in a .cshtml template for the po Extractor to find the strings to translate.
    /// </summary>
    /// <returns>Returns a list of localized strings</returns>
    public IDictionary<string, string> GetLocalizations(string[] groups)
    {
        var dictionary = new Dictionary<string, string>();

        if (groups.Contains("media-app"))
        {
            // Localizable Strings shared
            dictionary.Add("NewFolder", S["New folder"].Value);
            dictionary.Add("DeleteFolderTitle", S["Delete media folder"].Value);
            dictionary.Add("DeleteFolderMessage", S["Are you sure you want to delete this folder?"].Value);
            dictionary.Add("DeleteMediaTitle", S["Delete media"].Value);
            dictionary.Add("DeleteMediaMessage", S["Are you sure you want to delete this/these media item(s)?"].Value);
            dictionary.Add("RenameMediaTitle", S["Rename media"].Value);
            dictionary.Add("RenameMediaMessage", S["New name"].Value);
            dictionary.Add("MoveMediaTitle", S["Move media"].Value);
            dictionary.Add("MoveMediaMessage", S["Are you sure you want to move the selected media to this folder?"].Value);
            dictionary.Add("CreateFolderTitle", S["Create folder"].Value);
            dictionary.Add("CreateFolderMessage", S["Folder name"].Value);
            dictionary.Add("SameFolderMessage", S["The media is already on this folder"].Value);
            dictionary.Add("EditButton", S["Edit"].Value);
            dictionary.Add("DeleteButton", S["Delete"].Value);
            dictionary.Add("ViewButton", S["View"].Value);
            dictionary.Add("SelectAll", S["Select All"].Value);
            dictionary.Add("FolderEmpty", S["This folder is empty"].Value);
            dictionary.Add("FolderFilterEmpty", S["Nothing to show with this filter"].Value);
            dictionary.Add("Upload", S["Upload"].Value);
            dictionary.Add("SelectNone", S["Select None"].Value);
            dictionary.Add("Invert", S["Invert"].Value);
            dictionary.Add("Delete", S["Delete"].Value);
            dictionary.Add("DropHere", S["Drop your media here"].Value);
            dictionary.Add("DropTitle", S["Your files will be uploaded to the current folder when you drop them here"].Value);
            dictionary.Add("MediaLibrary", S["Media Library"].Value);
            dictionary.Add("Filter", S["Filter..."].Value);

            // Localizable Strings for uploadComponent
            dictionary.Add("Error", S["This file exceeds the maximum upload size"].Value);

            // Localizable Strings for uploadListcomponent
            dictionary.Add("Uploads", S["Uploads"].Value);
            dictionary.Add("Errors", S["Errors"].Value);
            dictionary.Add("ClearErrors", S["Clear Errors"].Value);

            // Localizable Strings for mediaItemsTableComponent
            dictionary.Add("ImageHeader", S["Image"].Value);
            dictionary.Add("NameHeader", S["Name"].Value);
            dictionary.Add("LastModifyHeader", S["Last modification"].Value);
            dictionary.Add("SizeHeader", S["Size"].Value);
            dictionary.Add("TypeHeader", S["Type"].Value);

            // Localizable Strings for pagerComponent
            dictionary.Add("PagerFirstButton", S["First"].Value);
            dictionary.Add("PagerPreviousButton", S["Previous"].Value);
            dictionary.Add("PagerNextButton", S["Next"].Value);
            dictionary.Add("PagerLastButton", S["Last"].Value);
            dictionary.Add("PagerPageSizeLabel", S["Page Size"].Value);
            dictionary.Add("PagerPageLabel", S["Page:"].Value);
            dictionary.Add("PagerTotalLabel", S["Total items:"].Value);

        }

        return dictionary;
    }
}
