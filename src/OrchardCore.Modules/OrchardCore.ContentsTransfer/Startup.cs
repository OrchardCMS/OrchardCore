using System;
using System.IO;
using CrestApps.Contents.Imports.Drivers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentsTransfer.Drivers;
using OrchardCore.ContentsTransfer.Handlers;
using OrchardCore.ContentsTransfer.Handlers.Fields;
using OrchardCore.ContentsTransfer.Indexes;
using OrchardCore.ContentsTransfer.Migrations;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentsTransfer.Services;
using OrchardCore.ContentTransfer;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Title.Models;

namespace OrchardCore.ContentsTransfer;

public class Startup : StartupBase
{
    private readonly AdminOptions _adminOptions;
    private readonly IShellConfiguration _configuration;

    public Startup(
        IOptions<AdminOptions> adminOptions,
        IShellConfiguration configuration)
    {
        _adminOptions = adminOptions.Value;
        _configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        // TODO, this should be moved to configuration.
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        services.AddSingleton<IContentTransferFileStore>(serviceProvider =>
        {
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
            var path = Path.Combine("App_Data", "Sites", shellSettings.Name, "Temp");
            var fileStore = new FileSystemStore(path);

            return new ContentTransferFileStore(fileStore);
        });

        services.AddDataMigration<ContentTransferMigrations>();
        services.AddIndexProvider<ContentTransferEntryIndexProvider>();
        services.AddScoped<IAuthorizationHandler, ContentTypeAuthorizationHandler>();
        services.AddScoped<IPermissionProvider, PermissionsProvider>();
        services.AddScoped<IDisplayDriver<ImportContent>, ImportContentDisplayDriver>();

        services.AddScoped<IContentImportHandlerResolver, ContentImportHandlerResolver>();
        services.AddScoped<IContentImportManager, ContentImportManager>();
        services.AddScoped<IContentTypeDefinitionDisplayDriver, ContentTypeTransferSettingsDisplayDriver>();
        services.AddScoped<IContentImportHandler, CommonContentImportHandler>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.Configure<ContentImportOptions>(_configuration.GetSection("OrchardCore_ContentsTransfer"));
        services.AddSingleton<IBackgroundTask, ImportFilesBackgroundTask>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "ImportContentFromFile",
            areaName: ContentTransferConstants.Feature.ModuleId,
            pattern: _adminOptions.AdminUrlPrefix + "/import/contents/{contentTypeId}",
            defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Import) }
        );

        routes.MapAreaControllerRoute(
            name: "ImportContentDownloadTemplateTemplate",
            areaName: ContentTransferConstants.Feature.ModuleId,
            pattern: _adminOptions.AdminUrlPrefix + "/import/contents/{contentTypeId}/download-template",
            defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.DownloadTemplate) }
        );

        routes.MapAreaControllerRoute(
            name: "ExportContentToFile",
            areaName: ContentTransferConstants.Feature.ModuleId,
            pattern: _adminOptions.AdminUrlPrefix + "/export/contents",
            defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.Export) }
        );

        routes.MapAreaControllerRoute(
            name: "ExportContentDownloadFile",
            areaName: ContentTransferConstants.Feature.ModuleId,
            pattern: _adminOptions.AdminUrlPrefix + "/export/contents/{contentTypeId}/download-file/{extension}",
            defaults: new { controller = typeof(AdminController).ControllerName(), action = nameof(AdminController.DownloadExport) }
        );
    }
}

[RequireFeatures("OrchardCore.Title")]
public class TitleStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<TitlePart, TitlePartContentImportHandler>();
    }
}

[RequireFeatures("OrchardCore.ContentFields")]
public class ContentFieldsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentFieldImportHandler<TextField, TextFieldImportHandler>();
        services.AddContentFieldImportHandler<NumericField, NumberFieldImportHandler>();
        services.AddContentFieldImportHandler<DateField, DateFieldImportHandler>();
        services.AddContentFieldImportHandler<DateTimeField, DateTimeFieldImportHandler>();
        services.AddContentFieldImportHandler<TimeField, TimeFieldImportHandler>();
        services.AddContentFieldImportHandler<ContentPickerField, ContentPickerFieldImportHandler>();
        services.AddContentFieldImportHandler<BooleanField, BooleanFieldImportHandler>();
    }
}
