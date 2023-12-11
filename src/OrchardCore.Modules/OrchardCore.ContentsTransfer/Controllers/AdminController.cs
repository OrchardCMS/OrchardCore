using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentsTransfer.Indexes;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentsTransfer.Services;
using OrchardCore.ContentsTransfer.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Media.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.ContentsTransfer.Controllers;

public class AdminController : Controller, IUpdateModel
{
    private static readonly JsonMergeSettings _updateJsonMergeSettings = new()
    {
        MergeArrayHandling = MergeArrayHandling.Replace,
        MergeNullValueHandling = MergeNullValueHandling.Ignore,
    };

    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;

    private readonly IDisplayManager<ContentTransferEntry> _entryDisplayManager;
    private readonly IContentTransferEntryAdminListQueryService _entriesAdminListQueryService;
    private readonly IDisplayManager<ListContentTransferEntryOptions> _entryOptionsDisplayManager;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;
    private readonly IClock _clock;
    private readonly IContentTransferFileStore _contentTransferFileStore;
    private readonly IContentManager _contentManager;
    private readonly IDisplayManager<ImportContent> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IContentImportManager _contentImportManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;

    protected readonly dynamic New;
    protected readonly IStringLocalizer S;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    protected readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        ISession session,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        IDisplayManager<ContentTransferEntry> entryDisplayManager,
        IContentTransferEntryAdminListQueryService entriesAdminListQueryService,
        IDisplayManager<ListContentTransferEntryOptions> entryOptionsDisplayManager,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer,
        IContentDefinitionManager contentDefinitionManager,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IContentTransferFileStore contentTransferFileStore,
        IContentManager contentManager,
        IDisplayManager<ImportContent> displayManager,
        IUpdateModelAccessor updateModelAccessor,
        IContentImportManager contentImportManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IClock clock)
    {
        _authorizationService = authorizationService;
        _session = session;
        New = shapeFactory;
        _entryDisplayManager = entryDisplayManager;
        _entriesAdminListQueryService = entriesAdminListQueryService;
        _entryOptionsDisplayManager = entryOptionsDisplayManager;
        _notifier = notifier;
        S = stringLocalizer;
        _contentDefinitionManager = contentDefinitionManager;
        H = htmlLocalizer;
        _contentTransferFileStore = contentTransferFileStore;
        _contentManager = contentManager;
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
        _contentImportManager = contentImportManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        _clock = clock;
    }

    public async Task<IActionResult> List(
        [ModelBinder(BinderType = typeof(ContentTransferEntryFilterEngineModelBinder), Name = "q")] QueryFilterResult<ContentTransferEntry> queryFilterResult,
        PagerParameters pagerParameters,
        ListContentTransferEntryOptions options)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ListContentTransferEntries))
        {
            return Forbid();
        }

        options.FilterResult = queryFilterResult;

        // The search text is provided back to the UI.
        options.SearchText = options.FilterResult.ToString();
        options.OriginalSearchText = options.SearchText;

        // Populate route values to maintain previous route data when generating page links.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        options.Statuses =
        [
            new(S["New"], nameof(ContentTransferEntryStatus.New)),
            new(S["Processing"], nameof(ContentTransferEntryStatus.Processing)),
            new(S["Completed"], nameof(ContentTransferEntryStatus.Completed)),
            new(S["Completed With Errors"], nameof(ContentTransferEntryStatus.CompletedWithErrors)),
            new(S["Failed"], nameof(ContentTransferEntryStatus.Failed)),
        ];

        options.Sorts =
        [
            new(S["Recently created"], nameof(ContentTransferEntryOrder.Latest)),
            new(S["Previously created"], nameof(ContentTransferEntryOrder.Oldest)),
        ];

        options.BulkActions =
        [
            new(S["Remove"], nameof(ContentTransferEntryBulkAction.Remove)),
        ];

        options.ImportableTypes = [];

        foreach (var contentTypeDefinition in await _contentDefinitionManager.ListTypeDefinitionsAsync())
        {
            var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();

            if (!settings.AllowBulkImport || !await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ImportContentFromFile, (object)contentTypeDefinition.Name))
            {
                continue;
            }

            options.ImportableTypes.Add(new SelectListItem(contentTypeDefinition.DisplayName, contentTypeDefinition.Name));
        }

        var routeData = new RouteData(options.RouteValues);
        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var queryResult = await _entriesAdminListQueryService.QueryAsync(pager.Page, pager.PageSize, options, this);

        var pagerShape = (await New.Pager(pager)).TotalItemCount(queryResult.TotalCount).RouteData(routeData);

        var summaries = new List<dynamic>();

        foreach (var entry in queryResult.Entries)
        {
            dynamic shape = await _entryDisplayManager.BuildDisplayAsync(entry, this, "SummaryAdmin");
            shape.ContentTransferEntry = entry;

            summaries.Add(shape);
        }

        var startIndex = (pagerShape.Page - 1) * pagerShape.PageSize + 1;
        options.StartIndex = startIndex;
        options.EndIndex = startIndex + summaries.Count - 1;
        options.EntriesCount = summaries.Count;
        options.TotalItemCount = pagerShape.TotalItemCount;

        var header = await _entryOptionsDisplayManager.BuildEditorAsync(options, this, false, string.Empty, string.Empty);

        var shapeViewModel = await _shapeFactory.CreateAsync<ListContentTransferEntriesViewModel>("ContentTransferEntriesAdminList", viewModel =>
        {
            viewModel.Options = options;
            viewModel.Header = header;
            viewModel.Entries = summaries;
            viewModel.Pager = pagerShape;
        });

        return View(shapeViewModel);
    }

    [HttpPost, ActionName(nameof(List))]
    [FormValueRequired("submit.Filter")]
    public async Task<ActionResult> ListFilterPOST(ListContentTransferEntryOptions options)
    {
        // When the user has typed something into the search input, no further evaluation of the form post is required.
        if (!string.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(List), new RouteValueDictionary { { "q", options.SearchText } });
        }

        // Evaluate the values provided in the form post and map them to the filter result and route values.
        await _entryOptionsDisplayManager.UpdateEditorAsync(options, this, false, string.Empty, string.Empty);

        // The route value must always be added after the editors have updated the models.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        return RedirectToAction(nameof(List), options.RouteValues);
    }

    [HttpPost, ActionName(nameof(List))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ListPOST(ListContentTransferEntryOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ListContentTransferEntries))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var notifications = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x => x.EntryId.IsIn(itemIds)).ListAsync();
            var utcNow = _clock.UtcNow;
            var counter = 0;

            switch (options.BulkAction)
            {
                case ContentTransferEntryBulkAction.Remove:
                    foreach (var notification in notifications)
                    {
                        _session.Delete(notification);
                        counter++;
                    }
                    if (counter > 0)
                    {
                        await _notifier.SuccessAsync(H["{0} {1} removed successfully.", counter, H.Plural(counter, "entry", "entries")]);
                    }
                    break;
                default:
                    break;
            }
        }

        return RedirectToAction(nameof(List));
    }

    public async Task<IActionResult> Delete(string entryId, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.DeleteContentTransferEntries))
        {
            return Forbid();
        }

        if (!string.IsNullOrWhiteSpace(entryId))
        {
            var entry = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x => x.EntryId == entryId).FirstOrDefaultAsync();

            if (entry != null)
            {
                _session.Delete(entry);
            }
        }

        return RedirectTo(returnUrl);
    }

    public async Task<IActionResult> Import(string contentTypeId)
    {
        if (string.IsNullOrEmpty(contentTypeId))
        {
            return NotFound();
        }

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentTypeId);

        if (contentTypeDefinition == null)
        {
            return NotFound();
        }

        var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();

        if (!settings.AllowBulkImport)
        {
            return BadRequest();
        }

        if (!await _authorizationService.AuthorizeAsync(User, ContentTransferPermissions.ImportContentFromFile, (object)contentTypeId))
        {
            return Unauthorized();
        }

        var context = new ImportContentContext()
        {
            ContentItem = await _contentManager.NewAsync(contentTypeId),
            ContentTypeDefinition = contentTypeDefinition,
        };

        var columns = await _contentImportManager.GetColumnsAsync(context);

        var importContent = new ImportContent()
        {
            ContentTypeId = contentTypeId,
            ContentTypeName = contentTypeDefinition.Name,
        };

        var viewModel = new ContentImporterViewModel()
        {
            ContentTypeDefinition = contentTypeDefinition,
            Content = await _displayManager.BuildEditorAsync(importContent, _updateModelAccessor.ModelUpdater, true, string.Empty, string.Empty),
            Columns = columns.Where(x => x.Type != ImportColumnType.ExportOnly),
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Import))]
    [ContentTransferSizeLimit]
    public async Task<IActionResult> ImportPOST(string contentTypeId)
    {
        if (string.IsNullOrEmpty(contentTypeId))
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, ContentTransferPermissions.ImportContentFromFile, (object)contentTypeId))
        {
            return Unauthorized();
        }

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentTypeId);

        if (contentTypeDefinition == null)
        {
            return NotFound();
        }

        var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();

        if (!settings.AllowBulkImport)
        {
            return NotFound();
        }

        var importContent = new ImportContent()
        {
            ContentTypeId = contentTypeId,
            ContentTypeName = contentTypeDefinition.Name,
        };

        var shape = await _displayManager.UpdateEditorAsync(importContent, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        if (ModelState.IsValid)
        {
            var extension = Path.GetExtension(importContent.File.FileName);

            // Create entry in the database
            var fileName = Guid.NewGuid() + extension;

            var storedFileName = await _contentTransferFileStore.CreateFileFromStreamAsync(fileName, importContent.File.OpenReadStream(), false);

            var entry = new ContentTransferEntry()
            {
                EntryId = IdGenerator.GenerateId(),
                ContentType = contentTypeId,
                Owner = CurrentUserId(),
                Author = User.Identity.Name,
                UploadedFileName = importContent.File.FileName,
                StoredFileName = storedFileName,
                Status = ContentTransferEntryStatus.New,
                CreatedUtc = _clock.UtcNow,
            };

            _session.Save(entry);

            await _notifier.SuccessAsync(H["The file was successfully added to the queue for processing."]);

            return RedirectToAction(nameof(List));
        }

        var context = new ImportContentContext()
        {
            ContentItem = await _contentManager.NewAsync(contentTypeId),
            ContentTypeDefinition = contentTypeDefinition,
        };

        var columns = await _contentImportManager.GetColumnsAsync(context);

        var viewModel = new ContentImporterViewModel()
        {
            ContentTypeDefinition = contentTypeDefinition,
            Content = shape,
            Columns = columns.Where(x => x.Type != ImportColumnType.ExportOnly),
        };

        return View(viewModel);
    }

    public async Task<IActionResult> DownloadTemplate(string contentTypeId)
    {
        if (string.IsNullOrEmpty(contentTypeId))
        {
            return NotFound();
        }

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentTypeId);

        if (contentTypeDefinition == null)
        {
            return NotFound();
        }

        var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();

        if (!settings.AllowBulkImport)
        {
            return BadRequest();
        }

        if (!await _authorizationService.AuthorizeAsync(User, ContentTransferPermissions.ImportContentFromFile, (object)contentTypeId))
        {
            return Unauthorized();
        }

        var context = new ImportContentContext()
        {
            ContentItem = await _contentManager.NewAsync(contentTypeId),
            ContentTypeDefinition = contentTypeDefinition,
        };

        var columns = await _contentImportManager.GetColumnsAsync(context);

        var content = new MemoryStream();
        using var package = new ExcelPackage(content);
        var workSheet = package.Workbook.Worksheets.Add(contentTypeDefinition.DisplayName);
        var columnIndex = 1;

        foreach (var column in columns)
        {
            if (column.Type == ImportColumnType.ExportOnly)
            {
                continue;
            }

            var cell = workSheet.Cells[1, columnIndex++];

            cell.Value = column.Name;
            cell.Style.Font.Bold = true;
            var description = column.Description;

            if (column.ValidValues != null && column.ValidValues.Length > 0)
            {
                description += "Valid values are: " + string.Join(" | ", column.ValidValues);
            }

            cell.AddComment(description);
        }

        package.Save();
        content.Seek(0, SeekOrigin.Begin);

        return new FileStreamResult(content, "application/octet-stream");
    }

    public async Task<IActionResult> Export()
    {
        var viewModel = new ContentExporterViewModel()
        {
            ContentTypes = new List<SelectListItem>(),
            Extensions = new List<SelectListItem>()
            {
                new (S["Excel Workbook"], ".xlsx"),
            },
            Extension = ".xlsx",
        };

        foreach (var contentTypeDefinition in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
        {
            var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();

            if (!settings.AllowBulkExport
                || !await _authorizationService.AuthorizeAsync(User, ContentTransferPermissions.ExportContentFromFile, (object)contentTypeDefinition.Name))
            {
                continue;
            }

            viewModel.ContentTypes.Add(new SelectListItem(contentTypeDefinition.DisplayName, contentTypeDefinition.Name));
        }

        if (viewModel.ContentTypes.Count == 0)
        {
            return BadRequest();
        }

        return View(viewModel);
    }

    public async Task<IActionResult> DownloadExport(string contentTypeId)
    {
        if (string.IsNullOrEmpty(contentTypeId))
        {
            return NotFound();
        }

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentTypeId);

        if (contentTypeDefinition == null)
        {
            return NotFound();
        }

        var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();

        if (!settings.AllowBulkExport)
        {
            return BadRequest();
        }

        if (!await _authorizationService.AuthorizeAsync(User, ContentTransferPermissions.ExportContentFromFile, (object)contentTypeId))
        {
            return Unauthorized();
        }

        var context = new ImportContentContext()
        {
            ContentItem = await _contentManager.NewAsync(contentTypeId),
            ContentTypeDefinition = contentTypeDefinition,
        };

        var columns = await _contentImportManager.GetColumnsAsync(context);

        var dataTable = new DataTable();

        foreach (var column in columns)
        {
            if (column.Type == ImportColumnType.ImportOnly)
            {
                continue;
            }

            dataTable.Columns.Add(column.Name);
        }

        var contentItems = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeId && x.Published)
            .OrderBy(x => x.PublishedUtc)
            .ListAsync();

        foreach (var contentItem in contentItems)
        {
            var mapContext = new ContentExportContext()
            {
                ContentItem = contentItem,
                ContentTypeDefinition = contentTypeDefinition,
                Row = dataTable.NewRow(),
            };

            await _contentImportManager.ExportAsync(mapContext);

            dataTable.Rows.Add(mapContext.Row);
        }

        var content = new MemoryStream();
        using var package = new ExcelPackage(content);
        var worksheet = package.Workbook.Worksheets.Add(contentTypeDefinition.DisplayName);
        worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

        worksheet.Protection.IsProtected = false;
        worksheet.Protection.AllowSelectLockedCells = false;
        await package.SaveAsync();
        content.Seek(0, SeekOrigin.Begin);

        // TODO, download the file in a specific format.
        return new FileStreamResult(content, "application/octet-stream");
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private IActionResult RedirectTo(string returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? (IActionResult)this.LocalRedirect(returnUrl, true)
            : RedirectToAction(nameof(List));
    }
}
