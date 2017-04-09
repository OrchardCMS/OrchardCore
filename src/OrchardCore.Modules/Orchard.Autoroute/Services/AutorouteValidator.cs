using Microsoft.Extensions.Localization;
using Orchard.Autoroute.Model;
using Orchard.ContentManagement.Records;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Autoroute.Services
{
    public class AutorouteValidator : IAutorouteValidator
    {
        private readonly ISession _session;
        private readonly IStringLocalizer<AutorouteValidator> T;

        public AutorouteValidator(ISession session, IStringLocalizer<AutorouteValidator> localizer)
        {
            _session = session;
            T = localizer;
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

            if (await _session.QueryIndexAsync<AutoroutePartIndex>(o => o.Path == autoroute.Path && o.ContentItemId != autoroute.ContentItem.ContentItemId).Count() > 0)
            {
                reportError(nameof(autoroute.Path), T["Your permalink is already in use."]);
            }
        }
    }
}
