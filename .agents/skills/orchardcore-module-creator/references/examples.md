# Module Examples Reference

## Example: Simple Settings Module

A module that adds site-wide settings.

### Manifest.cs
```csharp
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Site Settings",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.SiteSettings",
    Name = "Site Settings",
    Description = "Provides custom site settings.",
    Category = "Configuration"
)]
```

### Models/SiteSettings.cs
```csharp
namespace OrchardCore.SiteSettings.Models;

public class SiteSettingsModel
{
    public string ApiKey { get; set; }
    public bool EnableFeature { get; set; }
    public int MaxItems { get; set; } = 10;
}
```

### Startup.cs
```csharp
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.SiteSettings.Drivers;
using OrchardCore.SiteSettings.Navigation;

namespace OrchardCore.SiteSettings;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<SiteSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
    }
}
```

---

## Example: Content Part Module

A module that adds a "Rating" part to content items.

### Models/RatingPart.cs
```csharp
using OrchardCore.ContentManagement;

namespace OrchardCore.Rating.Models;

public class RatingPart : ContentPart
{
    public int Stars { get; set; }
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
}
```

### ViewModels/RatingPartViewModel.cs
```csharp
namespace OrchardCore.Rating.ViewModels;

public class RatingPartViewModel
{
    public int Stars { get; set; }
    public int ReviewCount { get; set; }
    public decimal AverageRating { get; set; }
}
```

### Views/RatingPart.cshtml
```html
@model OrchardCore.Rating.ViewModels.RatingPartViewModel

<div class="rating-display">
    <span class="stars">@Model.Stars / 5</span>
    <span class="review-count">(@Model.ReviewCount reviews)</span>
    <span class="average">Average: @Model.AverageRating.ToString("F1")</span>
</div>
```

### Views/RatingPart_Edit.cshtml
```html
@model OrchardCore.Rating.ViewModels.RatingPartViewModel

<div class="mb-3">
    <label asp-for="Stars" class="form-label">Stars (1-5)</label>
    <input asp-for="Stars" type="number" min="1" max="5" class="form-control" />
</div>

<div class="mb-3">
    <label asp-for="ReviewCount" class="form-label">Review Count</label>
    <input asp-for="ReviewCount" type="number" min="0" class="form-control" />
</div>
```

---

## Example: Admin Controller Module

A module with admin pages.

### Controllers/AdminController.cs
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;

namespace OrchardCore.YourModule.Controllers;

[Admin("YourModule/{action}/{id?}", "YourModule.{action}")]
public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;

    public AdminController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageYourFeature))
        {
            return Forbid();
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(YourViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageYourFeature))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create logic here
        
        return RedirectToAction(nameof(Index));
    }
}
```

### Views/Admin/Index.cshtml
```html
@{
    ViewLayout = "~/Views/Shared/_Layout.cshtml";
}

<zone Name="Title"><h1>Your Module</h1></zone>

<zone Name="Content">
    <div class="card">
        <div class="card-body">
            <p>Your module content here.</p>
            <a asp-action="Create" class="btn btn-primary">Create New</a>
        </div>
    </div>
</zone>
```

---

## Example: API Controller Module

A module exposing REST API endpoints.

### Controllers/ApiController.cs
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;

namespace OrchardCore.YourModule.Controllers;

[Route("api/yourmodule")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api")]
[IgnoreAntiforgeryToken]
public sealed class ApiController : ControllerBase
{
    private readonly IContentManager _contentManager;

    public ApiController(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var contentItem = await _contentManager.GetAsync(id);
        
        if (contentItem == null)
        {
            return NotFound();
        }

        return Ok(contentItem);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest request)
    {
        var contentItem = await _contentManager.NewAsync(request.ContentType);
        contentItem.DisplayText = request.Title;
        
        await _contentManager.CreateAsync(contentItem, VersionOptions.Published);
        
        return CreatedAtAction(nameof(Get), new { id = contentItem.ContentItemId }, contentItem);
    }
}

public record CreateRequest(string ContentType, string Title);
```

---

## Frontend Assets

### Assets.json
```json
[
  {
    "action": "vite",
    "name": "your-module",
    "source": "Assets/",
    "tags": ["js", "css"]
  }
]
```

### Assets/package.json
```json
{
  "name": "@orchardcore/your-module",
  "version": "1.0.0",
  "private": true,
  "dependencies": {
    "vue": "3.5.13"
  }
}
```

### Assets/js/main.js
```javascript
document.addEventListener('DOMContentLoaded', function() {
    console.log('Your module loaded');
});
```

### Assets/scss/styles.scss
```scss
.your-module {
    &__container {
        padding: 1rem;
    }
    
    &__title {
        font-size: 1.5rem;
        font-weight: bold;
    }
}
```
