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
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentsTransfer.ViewModels;
using OrchardCore.ContentTransfer;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.ContentsTransfer;

public class AdminController : Controller
{
    private static readonly JsonMergeSettings _updateJsonMergeSettings = new JsonMergeSettings
    {
        MergeArrayHandling = MergeArrayHandling.Replace,
        MergeNullValueHandling = MergeNullValueHandling.Ignore,
    };


    private readonly IDisplayManager<ImportContent> _displayManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentImportManager _contentImportManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly INotifier _notifier;
    private readonly ISession _session;
    private readonly IClock _clock;
    private readonly IContentManagerSession _contentManagerSession;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IContentTransferFileStore _contentTransferFileStore;
    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;
    protected readonly IHtmlLocalizer H;

    public AdminController(
        IDisplayManager<ImportContent> displayManager,
        IAuthorizationService authorizationService,
        IStringLocalizer<AdminController> stringLocalizer,
        IContentImportManager contentImportManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IContentManagerSession contentManagerSession,
        IUpdateModelAccessor updateModelAccessor,
        IContentTransferFileStore contentTransferFileStore,
        INotifier notifier,
        ISession session,
        IClock clock,
        ILogger<AdminController> logger)
    {
        _displayManager = displayManager;
        _authorizationService = authorizationService;
        S = stringLocalizer;
        _contentImportManager = contentImportManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _notifier = notifier;
        _session = session;
        _clock = clock;
        H = htmlLocalizer;
        _contentManagerSession = contentManagerSession;
        _updateModelAccessor = updateModelAccessor;
        _contentTransferFileStore = contentTransferFileStore;
        _logger = logger;
    }

    public async Task<IActionResult> Import(string contentTypeId)
    {
        if (string.IsNullOrEmpty(contentTypeId))
        {
            return NotFound();
        }

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);

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
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = ContentImportOptions.AbsoluteMaxAllowedFileSizeInBytes, ValueCountLimit = ContentImportOptions.AbsoluteMaxAllowedFileSizeInBytes)]
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

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);

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
                UploadedFileName = importContent.File.FileName,
                StoredFileName = storedFileName,
                Status = ContentTransferEntryStatus.New,
                CreatedUtc = _clock.UtcNow,
            };

            _session.Save(entry);

            await _notifier.SuccessAsync(H["The file was successfully added to the queue for processing."]);

            return RedirectToAction(nameof(Import));
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

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);

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
                new (S["CSV (comma delimited)"], ".csv"),
            },
            Extension = ".xlsx",
        };

        foreach (var contentTypeDefinition in _contentDefinitionManager.LoadTypeDefinitions())
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

        var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeId);

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
            var mapContext = new ContentExportMapContext()
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
        package.Save();
        content.Seek(0, SeekOrigin.Begin);

        // TODO, download the file in a specific format.
        return new FileStreamResult(content, "application/octet-stream");
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier);
}
