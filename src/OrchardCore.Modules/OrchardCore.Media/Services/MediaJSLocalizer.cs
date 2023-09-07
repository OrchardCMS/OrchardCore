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
            dictionary.Add("DeleteFolderTitle", S["Delete media folder"].Value);
            dictionary.Add("DeleteFolderMessage", S["Are you sure you want to delete this folder?"].Value);
            dictionary.Add("DeleteMediaTitle", S["Delete media"].Value);
            dictionary.Add("DeleteMediaMessage", S["Are you sure you want to delete these media items?"].Value);
            dictionary.Add("MoveMediaTitle", S["Move media"].Value);
            dictionary.Add("MoveMediaMessage", S["Are you sure you want to move the selected media to this folder?"].Value);
            dictionary.Add("SameFolderMessage", S["The media is already on this folder"].Value);
            dictionary.Add("t-edit-button", S["Edit"].Value);
            dictionary.Add("t-delete-button", S["Delete"].Value);
            dictionary.Add("t-view-button", S["View"].Value);
            dictionary.Add("SelectAll", S["Select All"].Value);
            //Select None
            //Invert
            //Delete
            //Drop your media here
            //Your files will be uploaded to the current folder when you drop them here
            //Upload

            // Localizable Strings for uploadComponent
            dictionary.Add("t-error", S["This file exceeds the maximum upload size"].Value);

            // Localizable Strings for uploadListcomponent
            dictionary.Add("t-uploads", S["Uploads"].Value);
            dictionary.Add("t-errors", S["Errors"].Value); // Duplicate here, need to rename
            dictionary.Add("t-clear-errors", S["Clear Errors"].Value);

            // Localizable Strings for mediaItemsTableComponent
            dictionary.Add("t-image-header", S["Image"].Value);
            dictionary.Add("t-name-header", S["Name"].Value);
            dictionary.Add("t-lastModify-header", S["Last modification"].Value);
            dictionary.Add("t-size-header", S["Size"].Value);
            dictionary.Add("t-type-header", S["Type"].Value);

            // Localizable Strings for pagerComponent
            dictionary.Add("t-pager-first-button", S["First"].Value);
            dictionary.Add("t-pager-previous-button", S["Previous"].Value);
            dictionary.Add("t-pager-next-button", S["Next"].Value);
            dictionary.Add("t-pager-last-button", S["Last"].Value);
            dictionary.Add("t-pager-page-size-label", S["Page Size"].Value);
            dictionary.Add("t-pager-page-label", S["Page:"].Value);
            dictionary.Add("t-pager-total-label", S["Total items:"].Value);

        }

        return dictionary;
    }
}
