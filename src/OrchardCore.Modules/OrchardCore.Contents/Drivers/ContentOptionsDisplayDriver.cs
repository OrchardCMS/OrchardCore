using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class ContentOptionsDisplayDriver : DisplayDriver<ContentOptionsViewModel>
    {
        // Maintain the Options prefix for compatability with binding.
        protected override void BuildPrefix(ContentOptionsViewModel model, string htmlFieldPrefix)
        {
            Prefix = "Options";
        }

        public override IDisplayResult Display(ContentOptionsViewModel model, IUpdateModel updater)
        {
            return Combine(
                Initialize<ContentOptionsViewModel>("ContentsAdminList__Search", m => BuildContentOptionsViewModel(m, model)).Location("Header", "Search:10"),
                Initialize<ContentOptionsViewModel>("ContentsAdminList__Create", m => BuildContentOptionsViewModel(m, model)).Location("Header", "Create:10"),
                Initialize<ContentOptionsViewModel>("ContentsAdminList__Summary", m => BuildContentOptionsViewModel(m, model)).Location("Header", "Summary:10"),
                Initialize<ContentOptionsViewModel>("ContentsAdminList__Filters", m => BuildContentOptionsViewModel(m, model)).Location("Header", "Actions:10.1"),
                Initialize<ContentOptionsViewModel>("ContentsAdminList_Fields_BulkActions", m => BuildContentOptionsViewModel(m, model)).Location("Header", "Actions:10.1"),
                Initialize<ContentOptionsViewModel>("ContentsAdminList__BulkActions", m => BuildContentOptionsViewModel(m, model)).Location("BulkActions", "Content:10")
            );
        }

        private static void BuildContentOptionsViewModel(ContentOptionsViewModel m, ContentOptionsViewModel model)
        {
                m.ContentTypeOptions = model.ContentTypeOptions;
                m.ContentStatuses = model.ContentStatuses;
                m.ContentSorts = model.ContentSorts;
                m.ContentsBulkAction = model.ContentsBulkAction;
                m.CreatableTypes = model.CreatableTypes;
                m.StartIndex = model.StartIndex;
                m.EndIndex = model.EndIndex;
                m.ContentItemsCount = model.ContentItemsCount;
                m.TotalItemCount = model.TotalItemCount;
        }
    }
}
