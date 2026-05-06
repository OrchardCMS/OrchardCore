using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Alias.Models;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.Autoroute.Models;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentTransfer.BackgroundTasks;
using OrchardCore.ContentTransfer.Drivers;
using OrchardCore.ContentTransfer.Handlers;
using OrchardCore.ContentTransfer.Handlers.Fields;
using OrchardCore.ContentTransfer.Indexes;
using OrchardCore.ContentTransfer.Migrations;
using OrchardCore.ContentTransfer.Models;
using OrchardCore.ContentTransfer.Services;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Html.Models;
using OrchardCore.Liquid.Models;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.Models;
using OrchardCore.Media.Fields;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.PublishLater.Models;
using OrchardCore.Security.Permissions;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Title.Models;
using YesSql.Filters.Query;

namespace OrchardCore.ContentTransfer;

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

[RequireFeatures("OrchardCore.Markdown")]
public sealed class MarkdownStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<MarkdownBodyPart, MarkdownBodyPartContentImportHandler>();
        services.AddContentFieldImportHandler<MarkdownField, MarkdownFieldImportHandler>();
    }
}

[RequireFeatures("OrchardCore.Alias")]
public sealed class AliasStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<AliasPart, AliasPartContentImportHandler>();
    }
}

[RequireFeatures("OrchardCore.ArchiveLater")]
public sealed class ArchiveLaterStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<ArchiveLaterPart, ArchiveLaterPartContentImportHandler>();
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
        services.AddContentFieldImportHandler<HtmlField, HtmlFieldImportHandler>();
        services.AddContentFieldImportHandler<MultiTextField, MultiTextFieldImportHandler>();
        services.AddContentFieldImportHandler<LinkField, LinkFieldImportHandler>();
        services.AddContentFieldImportHandler<YoutubeField, YoutubeFieldImportHandler>();
        services.AddContentFieldImportHandler<UserPickerField, UserPickerFieldImportHandler>();
    }
}

[RequireFeatures("OrchardCore.Liquid")]
public sealed class LiquidStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<LiquidPart, LiquidPartContentImportHandler>();
    }
}

[RequireFeatures("OrchardCore.Media")]
public sealed class MediaStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentFieldImportHandler<MediaField, MediaFieldImportHandler>();
    }
}

[RequireFeatures("OrchardCore.PublishLater")]
public sealed class PublishLaterStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPartImportHandler<PublishLaterPart, PublishLaterPartContentImportHandler>();
    }
}

[RequireFeatures("OrchardCore.Taxonomies")]
public sealed class TaxonomiesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentFieldImportHandler<TaxonomyField, TaxonomyFieldImportHandler>();
    }
}

[RequireFeatures("OrchardCore.ContentLocalization")]
public sealed class ContentLocalizationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentFieldImportHandler<LocalizationSetContentPickerField, LocalizationSetContentPickerFieldImportHandler>();
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
