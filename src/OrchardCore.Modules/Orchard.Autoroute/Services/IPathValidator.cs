using Microsoft.Extensions.Localization;
using Orchard.Autoroute.Model;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Autoroute.Services
{
    public interface IAutorouteValidator
    {
        Task ValidateAsync(AutoroutePart autoroute, Action<string, string> reportError);
        Task<bool> IsUniqueAsync(string path, AutoroutePart context);
    }

    public class AutorouteValidator : IAutorouteValidator
    {
        private readonly ISession _session;
        private readonly IStringLocalizer<AutorouteValidator> T;

        public AutorouteValidator(ISession session, IStringLocalizer<AutorouteValidator> localizer)
        {
            _session = session;
            T = localizer;
        }

        public async Task<bool> IsUniqueAsync(string path, AutoroutePart context)
        {
            var otherItemsWithSamePath = await _session.QueryIndexAsync<AutoroutePartIndex>(o => o.Path == path && o.ContentItemId != context.ContentItem.ContentItemId).List();
            if (otherItemsWithSamePath.Count() > 0)
            {
                return false;
            }

            return true;
        }

        public async Task ValidateAsync(AutoroutePart autoroute, Action<string, string> reportError)
        {
            if (!Regex.IsMatch(autoroute.Path, @"^[^:?#\[\]@!$&'()*+,.;=\s\""\<\>\\\|%]+$"))
            {
                reportError(nameof(autoroute.Path), T["Please do not use any of the following characters in your permalink: \":\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\", \", \"<\", \">\", \"\\\", \"|\", \"%\", \".\". No spaces are allowed (please use dashes or underscores instead)."]);
            }

            if (autoroute.Path.Length > 1850)
            {
                reportError(nameof(autoroute.Path), T["Your permalink is too long. The permalink can only be up to 1,850 characters."]);
            }

            if (!await IsUniqueAsync(autoroute.Path, autoroute))
            {
                reportError(nameof(autoroute.Path), T["Your parmalink is already in use."]);
            }
        }
    }
}
