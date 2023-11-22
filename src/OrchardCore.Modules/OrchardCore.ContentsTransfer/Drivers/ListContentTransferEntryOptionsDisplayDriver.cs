using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentsTransfer.Drivers;

public class ListContentTransferEntryOptionsDisplayDriver : DisplayDriver<ListContentTransferEntryOptions>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ListContentTransferEntryOptionsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    // Maintain the Options prefix for compatibility with binding.
    protected override void BuildPrefix(ListContentTransferEntryOptions model, string htmlFieldPrefix)
    {
        Prefix = "Options";
    }

    public override IDisplayResult Display(ListContentTransferEntryOptions model)
    {
        return Combine(
            Initialize<ListContentTransferEntryOptions>("ListContentTransferEntriesAdminListBulkActions", m => BuildOptionsViewModel(m, model))
                .Location("BulkActions", "Content:10"),
            View("ListContentTransferEntriesAdminFilters_Thumbnail__Status", model)
                .Location("Thumbnail", "Content:30"),
            View("ListContentTransferEntriesAdminFilters_Thumbnail__Sort", model)
                .Location("Thumbnail", "Content:40")
        );
    }

    public override IDisplayResult Edit(ListContentTransferEntryOptions model)
    {
        model.FilterResult.MapTo(model);

        return Combine(
            Initialize<ListContentTransferEntryOptions>("ListContentTransferEntriesAdminListBulkActions", m => BuildOptionsViewModel(m, model))
                .Location("BulkActions", "Content:10"),
            Initialize<ListContentTransferEntryOptions>("ListContentTransferEntriesAdminListSearch", m => BuildOptionsViewModel(m, model))
                .Location("Search:10"),
            Initialize<ListContentTransferEntryOptions>("ContentTransferEntriesAdminListImport", m => BuildOptionsViewModel(m, model))
                .Location("Create:10"),
            Initialize<ListContentTransferEntryOptions>("ContentTransferEntriesAdminListExport", m => BuildOptionsViewModel(m, model))
                .Location("Create:11")
                .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, ContentTransferPermissions.ExportContentFromFile)),
            Initialize<ListContentTransferEntryOptions>("ListContentTransferEntriesAdminListActionBarButtons", m => BuildOptionsViewModel(m, model))
                .Location("ActionBarButtons:10"),
            Initialize<ListContentTransferEntryOptions>("ListContentTransferEntriesAdminListSummary", m => BuildOptionsViewModel(m, model))
                .Location("Summary:10"),
            Initialize<ListContentTransferEntryOptions>("ListContentTransferEntriesAdminListFilters", m => BuildOptionsViewModel(m, model))
                .Location("Actions:10.1"),
            Initialize<ListContentTransferEntryOptions>("ListContentTransferEntriesAdminList_Fields_BulkActions", m => BuildOptionsViewModel(m, model))
                .Location("Actions:10.1")
        );
    }

    public override Task<IDisplayResult> UpdateAsync(ListContentTransferEntryOptions model, IUpdateModel updater)
    {
        // Map the incoming values from a form post to the filter result.
        model.FilterResult.MapFrom(model);

        return Task.FromResult(Edit(model));
    }

    private static void BuildOptionsViewModel(ListContentTransferEntryOptions m, ListContentTransferEntryOptions model)
    {
        m.Status = model.Status;
        m.SearchText = model.SearchText;
        m.OriginalSearchText = model.OriginalSearchText;
        m.FilterResult = model.FilterResult;
        m.Sorts = model.Sorts;
        m.Statuses = model.Statuses;
        m.BulkActions = model.BulkActions;
        m.ImportableTypes = model.ImportableTypes;
        m.StartIndex = model.StartIndex;
        m.EndIndex = model.EndIndex;
        m.TotalItemCount = model.TotalItemCount;
        m.OrderBy = model.OrderBy;
    }
}
