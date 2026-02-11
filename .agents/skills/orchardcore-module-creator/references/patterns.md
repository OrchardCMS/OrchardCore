# OrchardCore Patterns Reference

## Content Part

A content part is a reusable piece of content that can be attached to content types.

```csharp
using OrchardCore.ContentManagement;

namespace OrchardCore.YourModule.Models;

public class YourPart : ContentPart
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsEnabled { get; set; }
}
```

## Content Part Driver

Drivers handle display and editing of content parts.

```csharp
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.YourModule.Models;
using OrchardCore.YourModule.ViewModels;

namespace OrchardCore.YourModule.Drivers;

public sealed class YourPartDisplayDriver : ContentPartDisplayDriver<YourPart>
{
    public override IDisplayResult Display(YourPart part, BuildPartDisplayContext context)
    {
        return Initialize<YourPartViewModel>("YourPart", model =>
        {
            model.Title = part.Title;
            model.Description = part.Description;
        }).Location("Detail", "Content:5");
    }

    public override IDisplayResult Edit(YourPart part, BuildPartEditorContext context)
    {
        return Initialize<YourPartViewModel>("YourPart_Edit", model =>
        {
            model.Title = part.Title;
            model.Description = part.Description;
            model.IsEnabled = part.IsEnabled;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(
        YourPart part, 
        UpdatePartEditorContext context)
    {
        var viewModel = new YourPartViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);
        
        part.Title = viewModel.Title;
        part.Description = viewModel.Description;
        part.IsEnabled = viewModel.IsEnabled;
        
        return Edit(part, context);
    }
}
```

## Content Field

Custom fields for content types.

```csharp
using OrchardCore.ContentManagement;

namespace OrchardCore.YourModule.Fields;

public class YourField : ContentField
{
    public string Value { get; set; }
    public string[] Tags { get; set; }
}
```

## Migrations (YesSql)

Database migrations for creating indexes and tables.

```csharp
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.YourModule;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<YourIndex>(table => table
            .Column<string>("DocumentId", col => col.WithLength(26))
            .Column<string>("Name", col => col.WithLength(255))
            .Column<bool>("IsEnabled")
        );

        await SchemaBuilder.AlterIndexTableAsync<YourIndex>(table => table
            .CreateIndex("IDX_YourIndex_DocumentId", "DocumentId")
        );

        return 1;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<YourIndex>(table => table
            .AddColumn<DateTime>("CreatedUtc")
        );

        return 2;
    }
}
```

## Index Definition

```csharp
using YesSql.Indexes;

namespace OrchardCore.YourModule.Indexes;

public class YourIndex : MapIndex
{
    public string DocumentId { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
}

public class YourIndexProvider : IndexProvider<YourDocument>
{
    public override void Describe(DescribeContext<YourDocument> context)
    {
        context.For<YourIndex>()
            .Map(doc => new YourIndex
            {
                DocumentId = doc.Id,
                Name = doc.Name,
                IsEnabled = doc.IsEnabled,
            });
    }
}
```

## Permission Provider

```csharp
using OrchardCore.Security.Permissions;

namespace OrchardCore.YourModule;

public sealed class PermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageYourFeature = 
        new("ManageYourFeature", "Manage your feature");
    
    public static readonly Permission ViewYourFeature = 
        new("ViewYourFeature", "View your feature", [ManageYourFeature]);

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult<IEnumerable<Permission>>([ManageYourFeature, ViewYourFeature]);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = [ManageYourFeature],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = [ViewYourFeature],
        },
    ];
}
```

## Admin Menu

```csharp
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.YourModule;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Your Menu"], NavigationConstants.AdminMenuYourModulePriority, menu => menu
                .AddClass("your-module")
                .Id("yourmodule")
                .Add(S["Your Item"], S["Your Item"].PrefixPosition(), item => item
                    .Action("Index", "Admin", "OrchardCore.YourModule")
                    .Permission(PermissionProvider.ManageYourFeature)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
```

## Content Handler

```csharp
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.YourModule.Handlers;

public sealed class YourContentHandler : ContentHandlerBase
{
    public override Task PublishedAsync(PublishContentContext context)
    {
        // Handle content published event
        return Task.CompletedTask;
    }

    public override Task CreatedAsync(CreateContentContext context)
    {
        // Handle content created event
        return Task.CompletedTask;
    }

    public override Task RemovedAsync(RemoveContentContext context)
    {
        // Handle content removed event
        return Task.CompletedTask;
    }
}
```

## Background Task

```csharp
using OrchardCore.BackgroundTasks;

namespace OrchardCore.YourModule;

[BackgroundTask(Schedule = "*/15 * * * *", Description = "Runs every 15 minutes")]
public sealed class YourBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Background work implementation
        return Task.CompletedTask;
    }
}
```

## Service Registration in Startup.cs

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // Content part
    services.AddContentPart<YourPart>()
        .UseDisplayDriver<YourPartDisplayDriver>();
    
    // Content field
    services.AddContentField<YourField>()
        .UseDisplayDriver<YourFieldDisplayDriver>();
    
    // Services
    services.AddScoped<IYourService, YourService>();
    
    // Migrations
    services.AddDataMigration<Migrations>();
    
    // Index provider
    services.AddIndexProvider<YourIndexProvider>();
    
    // Permissions
    services.AddPermissionProvider<PermissionProvider>();
    
    // Navigation
    services.AddNavigationProvider<AdminMenu>();
    
    // Handlers
    services.AddContentHandler<YourContentHandler>();
}
```
