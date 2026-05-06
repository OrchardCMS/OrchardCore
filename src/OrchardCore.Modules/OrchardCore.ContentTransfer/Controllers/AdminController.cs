using System.Data;
using System.Security.Claims;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentTransfer.Indexes;
using OrchardCore.ContentTransfer.Models;
using OrchardCore.ContentTransfer.Services;
using OrchardCore.ContentTransfer.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.ContentTransfer.Controllers;

public sealed class AdminController : Controller, IUpdateModel
{
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
    private readonly IContentDefinitionManager _contentDefinitionManager;

    private readonly IStringLocalizer S;
    private readonly IHtmlLocalizer H;

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

    [Admin("content-transfer-entries", RouteName = "ListContentTransferEntries")]
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
        await PopulateListOptionsAsync(options, ContentTransferDirection.Import);

        return View(await BuildListViewModelAsync(options, pagerParameters));
    }

    [HttpPost]
    [ActionName(nameof(List))]
    [FormValueRequired("submit.Filter")]
    public async Task<ActionResult> ListFilterPOST(ListContentTransferEntryOptions options)
    {
        return await FilterListAsync(nameof(List), options);
    }

    [HttpPost]
    [ActionName(nameof(List))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ListPOST(ListContentTransferEntryOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ListContentTransferEntries))
        {
            return Forbid();
        }

        await ExecuteBulkActionAsync(itemIds, options.BulkAction, ContentTransferDirection.Import);

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
                if (!await DeleteEntryAsync(entry))
                {
                    await _notifier.ErrorAsync(H["The file for this transfer entry could not be deleted."]);
                    return RedirectTo(returnUrl);
                }

                await _session.SaveChangesAsync();
            }
        }

        return RedirectTo(returnUrl);
    }

    [Admin("import/contents/{contentTypeId}", "ImportContentFromFile")]
    public async Task<IActionResult> Import(string contentTypeId)
    {
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
                Direction = ContentTransferDirection.Import,
                CreatedUtc = _clock.UtcNow,
            };

            _session.Save(entry);
            await _session.SaveChangesAsync();
            await TriggerImportProcessingAsync(entry.EntryId);

            await _notifier.SuccessAsync(H["The file was successfully added to the queue and will start processing shortly."]);

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

    [Admin("import/contents/{contentTypeId}/download-template", "ImportContentDownloadTemplateTemplate")]
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
        using (var spreadsheetDocument = SpreadsheetDocument.Create(content, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = contentTypeDefinition.DisplayName?.Length > 31
                    ? contentTypeDefinition.DisplayName[..31]
                    : contentTypeDefinition.DisplayName,
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            var headerRow = new Row() { RowIndex = 1 };
            sheetData.Append(headerRow);

            uint columnIndex = 1;
            foreach (var column in columns)
            {
                if (column.Type == ImportColumnType.ExportOnly)
                {
                    continue;
                }

                var cell = new Cell()
                {
                    CellReference = GetCellReference(columnIndex, 1),
                    DataType = CellValues.String,
                    CellValue = new CellValue(column.Name),
                };
                headerRow.Append(cell);
                columnIndex++;
            }

            workbookPart.Workbook.Save();
        }

        content.Seek(0, SeekOrigin.Begin);

        return new FileStreamResult(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"{contentTypeDefinition.Name}_Template.xlsx",
        };
    }

    [Admin("export/contents", "ExportContentToFile")]
    public async Task<IActionResult> Export(
        [ModelBinder(BinderType = typeof(ContentTransferEntryFilterEngineModelBinder), Name = "q")] QueryFilterResult<ContentTransferEntry> queryFilterResult,
        PagerParameters pagerParameters,
        ListContentTransferEntryOptions options)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ExportContentFromFile))
        {
            return Forbid();
        }

        options.FilterResult = queryFilterResult;
        await PopulateListOptionsAsync(options, ContentTransferDirection.Export, CurrentUserId());

        return View(await BuildBulkExportViewModelAsync(options, pagerParameters));
    }

    [HttpPost]
    [ActionName(nameof(Export))]
    [FormValueRequired("submit.Filter")]
    public async Task<ActionResult> ExportFilterPOST(ListContentTransferEntryOptions options)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ExportContentFromFile))
        {
            return Forbid();
        }

        return await FilterListAsync(nameof(Export), options);
    }

    [HttpPost]
    [ActionName(nameof(Export))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ExportPOST(ListContentTransferEntryOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ExportContentFromFile))
        {
            return Forbid();
        }

        await ExecuteBulkActionAsync(itemIds, options.BulkAction, ContentTransferDirection.Export, CurrentUserId());

        return RedirectToAction(nameof(Export));
    }

    [Admin("export/contents/download-file", "ExportContentDownloadFile")]
    public async Task<IActionResult> DownloadExport(
        string contentTypeId,
        bool partialExport = false,
        DateTime? createdFrom = null,
        DateTime? createdTo = null,
        DateTime? modifiedFrom = null,
        DateTime? modifiedTo = null,
        string owners = null,
        bool publishedOnly = true,
        bool latestOnly = false,
        bool allVersions = false)
    {
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
        var exportColumns = columns.Where(x => x.Type != ImportColumnType.ImportOnly).ToList();

        // Build a filtered query for counting.
        var countQuery = BuildExportQuery(contentTypeId, partialExport, latestOnly, allVersions, createdFrom, createdTo, modifiedFrom, modifiedTo, owners);
        var totalCount = await countQuery.CountAsync();

        var contentImportOptions = HttpContext.RequestServices.GetRequiredService<IOptions<ContentImportOptions>>().Value;
        var threshold = contentImportOptions.ExportQueueThreshold;

        if (totalCount > threshold)
        {
            // Queue the export for background processing.
            var fileName = $"{contentTypeDefinition.Name}_Export_{Guid.NewGuid():N}.xlsx";

            var entry = new ContentTransferEntry()
            {
                EntryId = IdGenerator.GenerateId(),
                ContentType = contentTypeId,
                Owner = CurrentUserId(),
                Author = User.Identity.Name,
                UploadedFileName = $"{contentTypeDefinition.Name}_Export.xlsx",
                StoredFileName = fileName,
                Status = ContentTransferEntryStatus.New,
                Direction = ContentTransferDirection.Export,
                CreatedUtc = _clock.UtcNow,
            };

            // Store the filters so the background task can apply them.
            if (partialExport)
            {
                entry.Put(new ExportFilterPart
                {
                    PublishedOnly = publishedOnly,
                    LatestOnly = latestOnly,
                    AllVersions = allVersions,
                    CreatedFrom = createdFrom,
                    CreatedTo = createdTo,
                    ModifiedFrom = modifiedFrom,
                    ModifiedTo = modifiedTo,
                    Owners = owners,
                });
            }

            _session.Save(entry);
            await _session.SaveChangesAsync();
            await TriggerExportProcessingAsync(entry.EntryId);

            await _notifier.InformationAsync(H["The export contains {0} records and has been queued for background processing. You can download it from Bulk Export when it is ready.", totalCount]);

            return RedirectToAction(nameof(Export));
        }

        // Immediate export: write directly to a temp file stream using pagination.
        var batchSize = contentImportOptions.ExportBatchSize < 1 ? 200 : contentImportOptions.ExportBatchSize;

        var tempFilePath = Path.GetTempFileName();
        try
        {
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var spreadsheetDocument = SpreadsheetDocument.Create(fileStream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = contentTypeDefinition.DisplayName?.Length > 31
                        ? contentTypeDefinition.DisplayName[..31]
                        : contentTypeDefinition.DisplayName,
                };
                sheets.Append(sheet);

                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Write header row.
                var headerRow = new Row() { RowIndex = 1 };
                sheetData.Append(headerRow);

                uint columnIndex = 1;
                var columnNames = new List<string>();

                foreach (var column in exportColumns)
                {
                    var cell = new Cell()
                    {
                        CellReference = GetCellReference(columnIndex, 1),
                        DataType = CellValues.String,
                        CellValue = new CellValue(column.Name),
                    };
                    headerRow.Append(cell);
                    columnNames.Add(column.Name);
                    columnIndex++;
                }

                // Paginate content items and write each page directly.
                uint rowIndex = 2;
                var page = 0;

                while (true)
                {
                    var pageQuery = BuildExportQuery(contentTypeId, partialExport, latestOnly, allVersions, createdFrom, createdTo, modifiedFrom, modifiedTo, owners);

                    var contentItems = await pageQuery
                        .Skip(page * batchSize)
                        .Take(batchSize)
                        .ListAsync();

                    var items = contentItems.ToList();

                    if (items.Count == 0)
                    {
                        break;
                    }

                    // Create a temporary DataTable for this batch only.
                    using var dataTable = new DataTable();

                    foreach (var colName in columnNames)
                    {
                        dataTable.Columns.Add(colName);
                    }

                    foreach (var contentItem in items)
                    {
                        var mapContext = new ContentExportContext()
                        {
                            ContentItem = contentItem,
                            ContentTypeDefinition = contentTypeDefinition,
                            Row = dataTable.NewRow(),
                        };

                        await _contentImportManager.ExportAsync(mapContext);

                        var row = new Row() { RowIndex = rowIndex };
                        sheetData.Append(row);

                        columnIndex = 1;

                        foreach (var colName in columnNames)
                        {
                            var cellValue = mapContext.Row[colName]?.ToString() ?? string.Empty;
                            var cell = new Cell()
                            {
                                CellReference = GetCellReference(columnIndex, rowIndex),
                                DataType = CellValues.String,
                                CellValue = new CellValue(cellValue),
                            };
                            row.Append(cell);
                            columnIndex++;
                        }

                        rowIndex++;
                    }

                    page++;
                }

                workbookPart.Workbook.Save();
            }

            // Read back from temp file for download (file-based, not memory-based).
            var downloadStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, options: FileOptions.DeleteOnClose);

            return new FileStreamResult(downloadStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{contentTypeDefinition.Name}_Export.xlsx",
            };
        }
        catch
        {
            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Delete(tempFilePath);
            }

            throw;
        }
    }

    [Admin("export/dashboard", "ExportDashboard")]
    public Task<IActionResult> ExportDashboard(
        [ModelBinder(BinderType = typeof(ContentTransferEntryFilterEngineModelBinder), Name = "q")] QueryFilterResult<ContentTransferEntry> queryFilterResult,
        PagerParameters pagerParameters,
        ListContentTransferEntryOptions options)
    {
        return Export(queryFilterResult, pagerParameters, options);
    }

    [Admin("import/entries/{entryId}/process", "ProcessImport")]
    public async Task<IActionResult> ProcessImport(string entryId, string returnUrl)
    {
        if (string.IsNullOrEmpty(entryId))
        {
            return NotFound();
        }

        var entry = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x =>
                x.EntryId == entryId
                && x.Direction == ContentTransferDirection.Import
                && x.Owner == CurrentUserId())
            .FirstOrDefaultAsync();

        if (entry == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, ContentTransferPermissions.ImportContentFromFile, (object)entry.ContentType))
        {
            return Forbid();
        }

        if (entry.Status != ContentTransferEntryStatus.New && entry.Status != ContentTransferEntryStatus.Processing)
        {
            await _notifier.WarningAsync(H["Only new or processing import files can be processed again."]);
            return RedirectTo(returnUrl);
        }

        await TriggerImportProcessingAsync(entry.EntryId);
        await _notifier.SuccessAsync(H["The import file will be processed in the background shortly."]);

        return RedirectTo(returnUrl);
    }

    [Admin("import/entries/{entryId}/cancel", "CancelImport")]
    public async Task<IActionResult> CancelImport(string entryId, string returnUrl)
    {
        if (string.IsNullOrEmpty(entryId))
        {
            return NotFound();
        }

        var entry = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x =>
                x.EntryId == entryId
                && x.Direction == ContentTransferDirection.Import
                && x.Owner == CurrentUserId())
            .FirstOrDefaultAsync();

        if (entry == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, ContentTransferPermissions.ImportContentFromFile, (object)entry.ContentType))
        {
            return Forbid();
        }

        if (entry.Status != ContentTransferEntryStatus.New && entry.Status != ContentTransferEntryStatus.Processing)
        {
            await _notifier.WarningAsync(H["Only new or processing import files can be canceled."]);
            return RedirectTo(returnUrl);
        }

        var importedCount = entry.TryGet<ImportFileProcessStatsPart>(out var progressPart)
            ? progressPart.ImportedCount
            : 0;

        entry.Status = importedCount > 0
            ? ContentTransferEntryStatus.CanceledWithImportedRecords
            : ContentTransferEntryStatus.Canceled;
        entry.ProcessSaveUtc = _clock.UtcNow;
        entry.CompletedUtc = _clock.UtcNow;

        _session.Save(entry);
        await _session.SaveChangesAsync();

        await _notifier.SuccessAsync(importedCount > 0
            ? H["The import was canceled after some records had already been imported."]
            : H["The import was canceled before any records were imported."]);

        return RedirectTo(returnUrl);
    }

    [Admin("export/dashboard/{entryId}/download", "DownloadExportFile")]
    public async Task<IActionResult> DownloadExportFile(string entryId)
    {
        if (string.IsNullOrEmpty(entryId))
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ExportContentFromFile))
        {
            return Forbid();
        }

        var entry = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x =>
            x.EntryId == entryId
            && x.Direction == ContentTransferDirection.Export
            && x.Owner == CurrentUserId())
            .FirstOrDefaultAsync();

        if (entry == null || entry.Status != ContentTransferEntryStatus.Completed)
        {
            return NotFound();
        }

        var fileInfo = await _contentTransferFileStore.GetFileInfoAsync(entry.StoredFileName);

        if (fileInfo == null || fileInfo.Length == 0)
        {
            await _notifier.ErrorAsync(H["The export file is no longer available."]);
            return RedirectToAction(nameof(Export));
        }

        var stream = await _contentTransferFileStore.GetFileStreamAsync(fileInfo);

        return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = entry.UploadedFileName ?? $"{entry.ContentType}_Export.xlsx",
        };
    }

    [Admin("import/entries/{entryId}/download-errors", "DownloadErrors")]
    public async Task<IActionResult> DownloadErrors(string entryId)
    {
        if (string.IsNullOrEmpty(entryId))
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, ContentTransferPermissions.ImportContentFromFile))
        {
            return Forbid();
        }

        var entry = await _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x =>
            x.EntryId == entryId
            && x.Direction == ContentTransferDirection.Import
            && x.Owner == CurrentUserId())
            .FirstOrDefaultAsync();

        if (entry == null)
        {
            return NotFound();
        }

        if (!entry.TryGet<ImportFileProcessStatsPart>(out var statsPart)
            || statsPart.Errors == null
            || statsPart.Errors.Count == 0)
        {
            await _notifier.WarningAsync(H["No error records found for this entry."]);
            return RedirectToAction(nameof(List));
        }

        var fileInfo = await _contentTransferFileStore.GetFileInfoAsync(entry.StoredFileName);

        if (fileInfo == null || fileInfo.Length == 0)
        {
            await _notifier.ErrorAsync(H["The original import file is no longer available."]);
            return RedirectToAction(nameof(List));
        }

        await using var sourceStream = await _contentTransferFileStore.GetFileStreamAsync(fileInfo);

        using var sourceDoc = SpreadsheetDocument.Open(sourceStream, false);
        var sourceWorkbookPart = sourceDoc.WorkbookPart;
        var sourceSheet = sourceWorkbookPart.WorksheetParts.First().Worksheet;
        var sourceSheetData = sourceSheet.GetFirstChild<SheetData>();
        var sharedStringTable = sourceWorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
        var outputStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose | FileOptions.SequentialScan);
        using (var destDoc = SpreadsheetDocument.Create(outputStream, SpreadsheetDocumentType.Workbook))
        {
            var destWorkbookPart = destDoc.AddWorkbookPart();
            destWorkbookPart.Workbook = new Workbook();

            if (sharedStringTable != null)
            {
                var destSharedStringPart = destWorkbookPart.AddNewPart<SharedStringTablePart>();
                sharedStringTable.SharedStringTable.Save(destSharedStringPart);
            }

            var destWorksheetPart = destWorkbookPart.AddNewPart<WorksheetPart>();
            destWorksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = destDoc.WorkbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet()
            {
                Id = destDoc.WorkbookPart.GetIdOfPart(destWorksheetPart),
                SheetId = 1,
                Name = "Errors",
            });

            var destSheetData = destWorksheetPart.Worksheet.GetFirstChild<SheetData>();
            statsPart.ErrorMessages ??= [];

            var sourceRowIndex = 0;
            uint destRowIndex = 1;

            foreach (var sourceRow in sourceSheetData.Elements<Row>())
            {
                if (sourceRowIndex == 0)
                {
                    destSheetData.Append(CloneRowWithErrorMessage(sourceRow, destRowIndex, S["Errors"]));
                    destRowIndex++;
                    sourceRowIndex++;
                    continue;
                }

                if (statsPart.Errors.Contains(sourceRowIndex))
                {
                    statsPart.ErrorMessages.TryGetValue(sourceRowIndex, out var errorMessage);
                    destSheetData.Append(CloneRowWithErrorMessage(sourceRow, destRowIndex, errorMessage));
                    destRowIndex++;
                }

                sourceRowIndex++;
            }

            destWorkbookPart.Workbook.Save();
        }

        outputStream.Position = 0;

        return new FileStreamResult(outputStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"{entry.ContentType}_Errors.xlsx",
        };
    }

    private async Task PopulateListOptionsAsync(ListContentTransferEntryOptions options, ContentTransferDirection direction, string owner = null)
    {
        options.SearchText = options.FilterResult.ToString();
        options.OriginalSearchText = options.SearchText;
        options.Direction = direction;
        options.Owner = owner;
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        options.Statuses =
        [
            new(S["New"], nameof(ContentTransferEntryStatus.New)),
            new(S["Processing"], nameof(ContentTransferEntryStatus.Processing)),
            new(S["Completed"], nameof(ContentTransferEntryStatus.Completed)),
            new(S["Completed With Errors"], nameof(ContentTransferEntryStatus.CompletedWithErrors)),
            new(S["Canceled"], nameof(ContentTransferEntryStatus.Canceled)),
            new(S["Canceled With Imported Records"], nameof(ContentTransferEntryStatus.CanceledWithImportedRecords)),
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

        options.ImportableTypes = direction == ContentTransferDirection.Import
            ? await GetTransferableContentTypesAsync(ContentTransferDirection.Import)
            : [];
        options.ExportableTypes = direction == ContentTransferDirection.Export
            ? await GetTransferableContentTypesAsync(ContentTransferDirection.Export)
            : [];
    }

    private async Task<IList<SelectListItem>> GetTransferableContentTypesAsync(ContentTransferDirection direction)
    {
        var contentTypes = new List<SelectListItem>();

        foreach (var contentTypeDefinition in await _contentDefinitionManager.ListTypeDefinitionsAsync())
        {
            var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();
            var isAllowed = direction == ContentTransferDirection.Import
                ? settings.AllowBulkImport
                : settings.AllowBulkExport;
            var permission = direction == ContentTransferDirection.Import
                ? ContentTransferPermissions.ImportContentFromFile
                : ContentTransferPermissions.ExportContentFromFile;

            if (!isAllowed || !await _authorizationService.AuthorizeAsync(HttpContext.User, permission, (object)contentTypeDefinition.Name))
            {
                continue;
            }

            contentTypes.Add(new SelectListItem(contentTypeDefinition.DisplayName, contentTypeDefinition.Name));
        }

        return contentTypes;
    }

    private async Task<IShape> BuildListViewModelAsync(ListContentTransferEntryOptions options, PagerParameters pagerParameters)
    {
        var routeData = new RouteData(options.RouteValues);
        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var queryResult = await _entriesAdminListQueryService.QueryAsync(pager.Page, pager.PageSize, options, this);
        var pagerShape = await _shapeFactory.PagerAsync(pager, queryResult.TotalCount, routeData);

        var summaries = new List<dynamic>();

        foreach (var entry in queryResult.Entries)
        {
            dynamic shape = await _entryDisplayManager.BuildDisplayAsync(entry, this, "SummaryAdmin");
            shape.ContentTransferEntry = entry;
            summaries.Add(shape);
        }

        var startIndex = (pager.Page - 1) * pager.PageSize + 1;
        options.StartIndex = queryResult.TotalCount == 0 ? 0 : startIndex;
        options.EndIndex = queryResult.TotalCount == 0 ? 0 : startIndex + summaries.Count - 1;
        options.EntriesCount = summaries.Count;
        options.TotalItemCount = queryResult.TotalCount;

        var header = await _entryOptionsDisplayManager.BuildEditorAsync(options, this, false, string.Empty, string.Empty);

        return await _shapeFactory.CreateAsync<ListContentTransferEntriesViewModel>("ContentTransferEntriesAdminList", viewModel =>
        {
            viewModel.Options = options;
            viewModel.Header = header;
            viewModel.Entries = summaries;
            viewModel.Pager = pagerShape;
        });
    }

    private ContentExporterViewModel BuildContentExporterViewModel(IList<SelectListItem> exportableTypes)
        => new()
        {
            ContentTypes = exportableTypes,
            Extensions =
            [
                new(S["Excel Workbook"], ".xlsx"),
            ],
            Extension = ".xlsx",
        };

    private async Task<BulkExportViewModel> BuildBulkExportViewModelAsync(ListContentTransferEntryOptions options, PagerParameters pagerParameters)
    {
        return new BulkExportViewModel()
        {
            Exporter = BuildContentExporterViewModel(options.ExportableTypes),
            List = await BuildListViewModelAsync(options, pagerParameters),
        };
    }

    private async Task<ActionResult> FilterListAsync(string actionName, ListContentTransferEntryOptions options)
    {
        if (!string.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(actionName, new RouteValueDictionary
            {
                { "q", options.SearchText },
            });
        }

        await _entryOptionsDisplayManager.UpdateEditorAsync(options, this, false, string.Empty, string.Empty);
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        return RedirectToAction(actionName, options.RouteValues);
    }

    private async Task ExecuteBulkActionAsync(IEnumerable<string> itemIds, ContentTransferEntryBulkAction? bulkAction, ContentTransferDirection direction, string owner = null)
    {
        if (itemIds?.Any() != true)
        {
            return;
        }

        var query = _session.Query<ContentTransferEntry, ContentTransferEntryIndex>(x =>
            x.EntryId.IsIn(itemIds) && x.Direction == direction);

        if (!string.IsNullOrWhiteSpace(owner))
        {
            query = query.Where(x => x.Owner == owner);
        }

        var entries = await query.ListAsync();

        var deletedCount = 0;
        var failedCount = 0;

        switch (bulkAction)
        {
            case ContentTransferEntryBulkAction.Remove:
                foreach (var entry in entries)
                {
                    if (await DeleteEntryAsync(entry))
                    {
                        deletedCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }

                if (deletedCount > 0)
                {
                    await _session.SaveChangesAsync();
                    await _notifier.SuccessAsync(H["{0} {1} removed successfully.", deletedCount, H.Plural(deletedCount, "entry", "entries")]);
                }

                if (failedCount > 0)
                {
                    await _notifier.WarningAsync(H["{0} {1} could not be removed because the stored file could not be deleted.", failedCount, H.Plural(failedCount, "entry", "entries")]);
                }
                break;
        }
    }

    private async Task<bool> DeleteEntryAsync(ContentTransferEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.StoredFileName))
        {
            var fileInfo = await _contentTransferFileStore.GetFileInfoAsync(entry.StoredFileName);

            if (fileInfo != null && !await _contentTransferFileStore.TryDeleteFileAsync(entry.StoredFileName))
            {
                return false;
            }
        }

        _session.Delete(entry);

        return true;
    }

    private static Task TriggerImportProcessingAsync(string entryId)
        => HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(
            $"content-transfer-import-{entryId}",
            entryId,
            static (scope, id) => BackgroundTasks.ImportFilesBackgroundTask.ProcessEntriesAsync(scope.ServiceProvider, CancellationToken.None, id));

    private static Task TriggerExportProcessingAsync(string entryId)
        => HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(
            $"content-transfer-export-{entryId}",
            entryId,
            static (scope, id) => BackgroundTasks.ExportFilesBackgroundTask.ProcessEntriesAsync(scope.ServiceProvider, CancellationToken.None, id));

    private static Row CloneRowWithErrorMessage(Row sourceRow, uint destinationRowIndex, string errorMessage)
    {
        var destinationRow = new Row() { RowIndex = destinationRowIndex };
        uint columnIndex = 1;

        foreach (var sourceCell in sourceRow.Elements<Cell>())
        {
            var clonedCell = (Cell)sourceCell.CloneNode(true);
            clonedCell.CellReference = GetCellReference(columnIndex, destinationRowIndex);
            destinationRow.Append(clonedCell);
            columnIndex++;
        }

        destinationRow.Append(new Cell()
        {
            CellReference = GetCellReference(columnIndex, destinationRowIndex),
            DataType = CellValues.String,
            CellValue = new CellValue(errorMessage ?? string.Empty),
        });

        return destinationRow;
    }

    private static string GetCellReference(uint columnIndex, uint rowIndex)
    {
        var columnName = string.Empty;
        var dividend = columnIndex;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName + rowIndex;
    }

    private IQuery<ContentItem> BuildExportQuery(
        string contentTypeId,
        bool partialExport,
        bool latestOnly,
        bool allVersions,
        DateTime? createdFrom,
        DateTime? createdTo,
        DateTime? modifiedFrom,
        DateTime? modifiedTo,
        string owners)
    {
        IQuery<ContentItem, ContentItemIndex> query;

        if (allVersions)
        {
            query = _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeId);
        }
        else if (latestOnly)
        {
            query = _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeId && x.Latest);
        }
        else
        {
            // Default: published only.
            query = _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentTypeId && x.Published);
        }

        if (partialExport)
        {
            if (createdFrom.HasValue)
            {
                query = query.Where(x => x.CreatedUtc >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                query = query.Where(x => x.CreatedUtc <= createdTo.Value);
            }

            if (modifiedFrom.HasValue)
            {
                query = query.Where(x => x.ModifiedUtc >= modifiedFrom.Value);
            }

            if (modifiedTo.HasValue)
            {
                query = query.Where(x => x.ModifiedUtc <= modifiedTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(owners))
            {
                var ownerList = owners.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (ownerList.Length == 1)
                {
                    var owner = ownerList[0];
                    query = query.Where(x => x.Owner == owner);
                }
                else if (ownerList.Length > 1)
                {
                    // Capture into locals (safe for expression trees, supports up to 5 owners).
                    var o0 = ownerList[0];
                    var o1 = ownerList.ElementAtOrDefault(1) ?? o0;
                    var o2 = ownerList.ElementAtOrDefault(2) ?? o0;
                    var o3 = ownerList.ElementAtOrDefault(3) ?? o0;
                    var o4 = ownerList.ElementAtOrDefault(4) ?? o0;

                    query = query.Where(x =>
                        x.Owner == o0 || x.Owner == o1 || x.Owner == o2 ||
                        x.Owner == o3 || x.Owner == o4);
                }
            }
        }

        return query.OrderBy(x => x.CreatedUtc);
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
