using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using OrchardCore.ContentsTransfer.Services;
using OrchardCore.ContentTransfer;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Modules;

namespace OrchardCore.ContentsTransfer;

public class Startup : StartupBase
{
    private const string ContentFolderName = "__ContentTransfer";

    public override void ConfigureServices(IServiceCollection services)
    {
        // TODO, this should be moved to configuration.
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        services.AddSingleton<IContentTransferFileStore>(serviceProvider =>
        {
            var fileStore = new FileSystemStore(ContentFolderName);

            return new ContentTransferFileStore(fileStore);
        });
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "Home",
            areaName: "OrchardCore.ContentsTransfer",
            pattern: "Home/Index",
            defaults: new { controller = "Home", action = "Index" }
        );
    }
}
