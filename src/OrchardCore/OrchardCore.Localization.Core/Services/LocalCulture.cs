using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization.Services
{
    public class LocalCulture : ILocalCulture
    {
        private static readonly Task<CultureInfo> Empty = Task.FromResult<CultureInfo>(null);

        private readonly IEnumerable<ICultureSelector> _cultureSelectors;
        private Task<CultureInfo> _culture = Empty;

        public LocalCulture(IEnumerable<ICultureSelector> cultureSelectors)
        {
            _cultureSelectors = cultureSelectors;
        }

        public Task<CultureInfo> GetLocalCultureAsync()
        {
            // Caching the result per request
            if (_culture == Empty)
            {
                _culture = LoadLocalCultureAsync();
            }

            return _culture;
        }

        private async Task<CultureInfo> LoadLocalCultureAsync()
        {
            var cultureResults = new List<CultureSelectorResult>();

            foreach (var cultureSelector in _cultureSelectors)
            {
                var cultureResult = await cultureSelector.GetCultureAsync();

                if (cultureResult != null)
                {
                    cultureResults.Add(cultureResult);
                }
            }

            if (cultureResults.Count == 0)
            {
                return CultureInfo.InvariantCulture;
            }
            else if (cultureResults.Count > 1)
            {
                cultureResults.Sort((x, y) => y.Priority.CompareTo(x.Priority));
            }

            foreach (var result in cultureResults)
            {
                var value = await result.Name();

                if (!String.IsNullOrEmpty(value))
                {
                    return CultureInfo.GetCultureInfo(value);
                }
            }

            return CultureInfo.InvariantCulture;
        }
    }
}
