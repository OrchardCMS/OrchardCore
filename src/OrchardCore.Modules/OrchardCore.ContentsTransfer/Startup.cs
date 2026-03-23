using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentsTransfer.BackgroundTasks;
using OrchardCore.ContentsTransfer.Drivers;
using OrchardCore.ContentsTransfer.Handlers;
using OrchardCore.ContentsTransfer.Handlers.Fields;
using OrchardCore.ContentsTransfer.Indexes;
using OrchardCore.ContentsTransfer.Migrations;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentsTransfer.Services;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Html.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Title.Models;
using YesSql.Filters.Query;

namespace OrchardCore.ContentsTransfer;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _configuration;

    public Startup(IShellConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IContentTransferFileStore>(serviceProvider =>
        {
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
            var logger = serviceProvider.GetRequiredService<ILogger<FileSystemStore>>();
            var path = Path.Combine(ShellOptionConstants.DefaultAppDataPath, ShellOptionConstants.DefaultSitesPath, shellSettings.Name, "Temp");
            var fileStore = new FileSystemStore(path, logger);

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
        services.AddSingleton<IBackgroundTask, ExportFilesBackgroundTask>();

        services.AddScoped<IContentTransferEntryAdminListQueryService, DefaultContentTransferEntryAdminListQueryService>();
        services.AddScoped<IDisplayDriver<ListContentTransferEntryOptions>, ListContentTransferEntryOptionsDisplayDriver>();
        services.AddScoped<IDisplayDriver<ContentTransferEntry>, ContentTransferEntryDisplayDriver>();
        services.AddTransient<IContentTransferEntryAdminListFilterProvider, DefaultContentTransferEntryAdminListFilterProvider>();
        services.AddSingleton<IContentTransferEntryAdminListFilterParser>(sp =>
        {
            var filterProviders = sp.GetServices<IContentTransferEntryAdminListFilterProvider>();
            var builder = new QueryEngineBuilder<ContentTransferEntry>();
            foreach (var provider in filterProviders)
            {
                provider.Build(builder);
            }

            var parser = builder.Build();

            return new DefaultContentTypeEntryAdminListFilterParser(parser);
        });
    }
}

[RequireFeatures("OrchardCore.Title")]
public sealed class TitleStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<TitlePart, TitlePartContentImportHandler>();
    }
}

[RequireFeatures("OrchardCore.Html")]
public sealed class HtmlBodyStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<HtmlBodyPart, HtmlBodyPartContentImportHandler>();
    }
}

[RequireFeatures("OrchardCore.Autoroute")]
public sealed class AutorouteStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<AutoroutePart, AutoroutePartContentImportHandler>();
    }
}

[RequireFeatures("OrchardCore.ContentFields")]
public sealed class ContentFieldsStartup : StartupBase
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

[RequireFeatures("OrchardCore.Notifications")]
public sealed class NotificationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IContentTransferNotificationHandler, ContentTransferNotificationHandler>();
    }
}
