using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapHelperService : ISitemapHelperService
    {
        // Path requirements for sitemaps include . as acceptable character.
        public static char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,;=<>\\|%".ToCharArray();
        public const int MaxPathLength = 1024;
        public const string Prefix = "";
        public const string Path = "Path";
        public const string SitemapPathExtension = ".xml";

        private readonly ISlugService _slugService;
        private readonly ISitemapManager _sitemapManager;
        private readonly IStringLocalizer S;

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

            if (path.IndexOfAny(InvalidCharactersForPath) > -1 || path.IndexOf(' ') > -1 || path.IndexOf("//") > -1)
            {
                var invalidCharactersForMessage = string.Join(", ", InvalidCharactersForPath.Select(c => $"\"{c}\""));
                updater.ModelState.AddModelError(Prefix, Path, S["Please do not use any of the following characters in your permalink: {0}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead).", invalidCharactersForMessage]);
            }

            // Precludes possibility of collision with Autoroute as Autoroute excludes . as a valid path character.
            if (!path.EndsWith(SitemapPathExtension))
            {
                updater.ModelState.AddModelError(Prefix, Path, S["Your permalink must end with {0}.", SitemapPathExtension]);
            }

            if (path.Length > MaxPathLength)
            {
                updater.ModelState.AddModelError(Prefix, Path, S["Your permalink is too long. The permalink can only be up to {0} characters.", MaxPathLength]);
            }

            var routeExists = false;
            if (string.IsNullOrEmpty(sitemapId))
            {
                routeExists = (await _sitemapManager.ListSitemapsAsync())
                    .Any(p => String.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                routeExists = (await _sitemapManager.ListSitemapsAsync())
                    .Any(p => p.SitemapId != sitemapId && String.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase));
            }

            if (routeExists)
            {
                updater.ModelState.AddModelError(Prefix, Path, S["Your permalink is already in use."]);
            }
        }

        public string GetSitemapSlug(string name)
        {
            return _slugService.Slugify(name) + SitemapPathExtension;
        }

    }
}
