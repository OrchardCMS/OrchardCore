using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules.Services;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapHelperService : ISitemapHelperService
    {
        // Path requirements for sitemaps include . as acceptable character.
        public static readonly char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,;=<>\\|%".ToCharArray();
        public const int MaxPathLength = 1024;
        public const string Prefix = "";
        public const string Path = "Path";

        private readonly ISlugService _slugService;
        private readonly ISitemapManager _sitemapManager;
        protected readonly IStringLocalizer S;

        public SitemapHelperService(
            ISlugService slugService,
            ISitemapManager sitemapManager,
            IStringLocalizer<SitemapHelperService> stringLocalizer
            )
        {
            _slugService = slugService;
            _sitemapManager = sitemapManager;
            S = stringLocalizer;
        }

        public async Task ValidatePathAsync(string path, IUpdateModel updater, string sitemapId = null)
        {
            // Keep localized text as similar to Autoroute as possible.
            if (path == "/")
            {
                updater.ModelState.AddModelError(Prefix, Path, S["Your permalink can't be set to the homepage"]);
            }

            if (path.IndexOfAny(InvalidCharactersForPath) > -1 || path.IndexOf(' ') > -1)
            {
                var invalidCharactersForMessage = String.Join(", ", InvalidCharactersForPath.Select(c => $"\"{c}\""));
                updater.ModelState.AddModelError(Prefix, Path, S["Please do not use any of the following characters in your permalink: {0}. No spaces are allowed (please use dashes or underscores instead).", invalidCharactersForMessage]);
            }

            // Precludes possibility of collision with Autoroute as Autoroute excludes . as a valid path character.
            if (!path.EndsWith(Sitemap.PathExtension))
            {
                updater.ModelState.AddModelError(Prefix, Path, S["Your permalink must end with {0}.", Sitemap.PathExtension]);
            }

            if (path.Length > MaxPathLength)
            {
                updater.ModelState.AddModelError(Prefix, Path, S["Your permalink is too long. The permalink can only be up to {0} characters.", MaxPathLength]);
            }

            var routeExists = false;
            if (String.IsNullOrEmpty(sitemapId))
            {
                routeExists = (await _sitemapManager.GetSitemapsAsync())
                    .Any(p => String.Equals(p.Path, path.TrimStart('/'), StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                routeExists = (await _sitemapManager.GetSitemapsAsync())
                    .Any(p => p.SitemapId != sitemapId && String.Equals(p.Path, path.TrimStart('/'), StringComparison.OrdinalIgnoreCase));
            }

            if (routeExists)
            {
                updater.ModelState.AddModelError(Prefix, Path, S["Your permalink is already in use."]);
            }
        }

        public string GetSitemapSlug(string name)
        {
            return _slugService.Slugify(name) + Sitemap.PathExtension;
        }

    }
}
