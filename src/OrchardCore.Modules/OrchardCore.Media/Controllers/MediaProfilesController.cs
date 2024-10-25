using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using Format = OrchardCore.Media.Processing.Format;
using ResizeMode = OrchardCore.Media.Processing.ResizeMode;

namespace OrchardCore.Media.Controllers;

[Admin("MediaProfiles/{action}", "MediaProfiles.{action}")]
public sealed class MediaProfilesController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly MediaProfilesManager _mediaProfilesManager;
    private readonly MediaOptions _mediaOptions;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public MediaProfilesController(
        IAuthorizationService authorizationService,
        MediaProfilesManager mediaProfilesManager,
        IOptions<MediaOptions> mediaOptions,
        IOptions<PagerOptions> pagerOptions,
        INotifier notifier,
        IShapeFactory shapeFactory,
        IStringLocalizer<MediaProfilesController> stringLocalizer,
        IHtmlLocalizer<MediaProfilesController> htmlLocalizer
        )
    {
        _authorizationService = authorizationService;
        _mediaProfilesManager = mediaProfilesManager;
        _mediaOptions = mediaOptions.Value;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("MediaProfiles", "MediaProfiles.Index")]
    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var mediaProfilesDocument = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();
        var mediaProfiles = mediaProfilesDocument.MediaProfiles.ToList();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            mediaProfiles = mediaProfiles.Where(x => x.Key.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var count = mediaProfiles.Count;

        mediaProfiles = mediaProfiles.OrderBy(x => x.Key)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);

        var model = new MediaProfileIndexViewModel
        {
            MediaProfiles = mediaProfiles.Select(x => new MediaProfileEntry { Name = x.Key, MediaProfile = x.Value }).ToList(),
            Pager = pagerShape
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(MediaProfileIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search }
        });

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
        {
            return Forbid();
        }

        var model = new MediaProfileViewModel();

        BuildViewModel(model);

        return View(model);
    }

    [HttpPost, ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(MediaProfileViewModel model, string submit)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(MediaProfileViewModel.Name), S["The name is mandatory."]);
            }
            else
            {
                var mediaProfilesDocument = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

                if (mediaProfilesDocument.MediaProfiles.ContainsKey(model.Name))
                {
                    ModelState.AddModelError(nameof(MediaProfileViewModel.Name), S["A profile with the same name already exists."]);
                }
            }
        }

        if (ModelState.IsValid)
        {
            var isCustomWidth = model.SelectedWidth != 0 && Array.BinarySearch(_mediaOptions.SupportedSizes, model.SelectedWidth) < 0;
            var isCustomHeight = model.SelectedHeight != 0 && Array.BinarySearch(_mediaOptions.SupportedSizes, model.SelectedHeight) < 0;

            var mediaProfile = new MediaProfile
            {
                Hint = model.Hint,
                Width = isCustomWidth ? model.CustomWidth : model.SelectedWidth,
                Height = isCustomHeight ? model.CustomHeight : model.SelectedHeight,
                Mode = model.SelectedMode,
                Format = model.SelectedFormat,
                Quality = model.Quality,
                BackgroundColor = model.BackgroundColor
            };

            await _mediaProfilesManager.UpdateMediaProfileAsync(model.Name, mediaProfile);

            if (submit == "SaveAndContinue")
            {
                return RedirectToAction(nameof(Edit), new { name = model.Name });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // If we got this far, something failed, redisplay form
        BuildViewModel(model);

        return View(model);
    }

    public async Task<IActionResult> Edit(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
        {
            return Forbid();
        }

        var mediaProfilesDocument = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

        if (!mediaProfilesDocument.MediaProfiles.TryGetValue(name, out var mediaProfile))
        {
            return RedirectToAction(nameof(Create), new { name });
        }

        // Is a custom width if the width is not 0 and it is not in the array of supported sizes.
        var isCustomWidth = mediaProfile.Width != 0 && Array.BinarySearch(_mediaOptions.SupportedSizes, mediaProfile.Width) < 0;
        var isCustomHeight = mediaProfile.Height != 0 && Array.BinarySearch(_mediaOptions.SupportedSizes, mediaProfile.Height) < 0;

        var model = new MediaProfileViewModel
        {
            Name = name,
            Hint = mediaProfile.Hint,
            SelectedWidth = isCustomWidth ? -1 : mediaProfile.Width,
            CustomWidth = isCustomWidth ? mediaProfile.Width : 0,
            SelectedHeight = isCustomHeight ? -1 : mediaProfile.Height,
            CustomHeight = isCustomHeight ? mediaProfile.Height : 0,
            SelectedMode = mediaProfile.Mode,
            SelectedFormat = mediaProfile.Format,
            Quality = mediaProfile.Quality,
            BackgroundColor = mediaProfile.BackgroundColor
        };

        BuildViewModel(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string sourceName, MediaProfileViewModel model, string submit)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
        {
            return Forbid();
        }

        var mediaProfilesDocument = await _mediaProfilesManager.LoadMediaProfilesDocumentAsync();

        if (!mediaProfilesDocument.MediaProfiles.ContainsKey(sourceName))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(MediaProfileViewModel.Name), S["The name is mandatory."]);
            }
        }

        if (ModelState.IsValid)
        {
            var isCustomWidth = Array.BinarySearch(_mediaOptions.SupportedSizes, model.SelectedWidth) < 0;
            var isCustomHeight = Array.BinarySearch(_mediaOptions.SupportedSizes, model.SelectedHeight) < 0;

            var mediaProfile = new MediaProfile
            {
                Hint = model.Hint,
                Width = isCustomWidth ? model.CustomWidth : model.SelectedWidth,
                Height = isCustomHeight ? model.CustomHeight : model.SelectedHeight,
                Mode = model.SelectedMode,
                Format = model.SelectedFormat,
                Quality = model.Quality,
                BackgroundColor = model.BackgroundColor
            };

            await _mediaProfilesManager.RemoveMediaProfileAsync(sourceName);

            await _mediaProfilesManager.UpdateMediaProfileAsync(model.Name, mediaProfile);

            if (submit != "SaveAndContinue")
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // If we got this far, something failed, redisplay form
        BuildViewModel(model);

        // If the name was changed or removed, prevent a 404 or a failure on the next post.
        model.Name = sourceName;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
        {
            return Forbid();
        }

        var mediaProfilesDocument = await _mediaProfilesManager.LoadMediaProfilesDocumentAsync();

        if (!mediaProfilesDocument.MediaProfiles.ContainsKey(name))
        {
            return NotFound();
        }

        await _mediaProfilesManager.RemoveMediaProfileAsync(name);

        await _notifier.SuccessAsync(H["Media profile deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("Index")]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var mediaProfilesDocument = await _mediaProfilesManager.LoadMediaProfilesDocumentAsync();
            var checkedContentItems = mediaProfilesDocument.MediaProfiles.Where(x => itemIds.Contains(x.Key));
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        await _mediaProfilesManager.RemoveMediaProfileAsync(item.Key);
                    }
                    await _notifier.SuccessAsync(H["Media profiles successfully removed."]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(options.BulkAction.ToString(), "Invalid bulk action.");
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private void BuildViewModel(MediaProfileViewModel model)
    {
        model.AvailableWidths.Add(new SelectListItem() { Text = S["Default"], Value = "0" });
        model.AvailableHeights.Add(new SelectListItem() { Text = S["Default"], Value = "0" });
        model.AvailableWidths.AddRange(_mediaOptions.SupportedSizes.Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString() }));
        model.AvailableHeights.AddRange(_mediaOptions.SupportedSizes.Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString() }));
        if (_mediaOptions.UseTokenizedQueryString)
        {
            model.AvailableWidths.Add(new SelectListItem() { Text = S["Custom Size"], Value = "-1" });
            model.AvailableHeights.Add(new SelectListItem() { Text = S["Custom Size"], Value = "-1" });
        }

        model.AvailableResizeModes.Add(new SelectListItem() { Text = S["Default (Max)"], Value = ((int)ResizeMode.Undefined).ToString() });
        model.AvailableResizeModes.Add(new SelectListItem() { Text = S["Max"], Value = ((int)ResizeMode.Max).ToString() });
        model.AvailableResizeModes.Add(new SelectListItem() { Text = S["Crop"], Value = ((int)ResizeMode.Crop).ToString() });
        model.AvailableResizeModes.Add(new SelectListItem() { Text = S["Pad"], Value = ((int)ResizeMode.Pad).ToString() });
        model.AvailableResizeModes.Add(new SelectListItem() { Text = S["BoxPad"], Value = ((int)ResizeMode.BoxPad).ToString() });
        model.AvailableResizeModes.Add(new SelectListItem() { Text = S["Min"], Value = ((int)ResizeMode.Min).ToString() });
        model.AvailableResizeModes.Add(new SelectListItem() { Text = S["Stretch"], Value = ((int)ResizeMode.Stretch).ToString() });


        model.AvailableFormats.Add(new SelectListItem() { Text = S["Default"], Value = ((int)Format.Undefined).ToString() });
        model.AvailableFormats.Add(new SelectListItem() { Text = S["Bmp"], Value = ((int)Format.Bmp).ToString() });
        model.AvailableFormats.Add(new SelectListItem() { Text = S["Gif"], Value = ((int)Format.Gif).ToString() });
        model.AvailableFormats.Add(new SelectListItem() { Text = S["Jpg"], Value = ((int)Format.Jpg).ToString() });
        model.AvailableFormats.Add(new SelectListItem() { Text = S["Png"], Value = ((int)Format.Png).ToString() });
        model.AvailableFormats.Add(new SelectListItem() { Text = S["Tga"], Value = ((int)Format.Tga).ToString() });
        model.AvailableFormats.Add(new SelectListItem() { Text = S["WebP"], Value = ((int)Format.WebP).ToString() });
    }
}
