using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    public class DataLocalizer : IDataLocalizer
    {
        private IDataLocalizer _localizer;

        public DataLocalizer(IDataLocalizerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _localizer = factory.Create();
        }

        public LocalizedString this[string name] => _localizer[name];

        public LocalizedString this[string name, params object[] arguments] => _localizer[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => _localizer.GetAllStrings(includeParentCultures);

        public IStringLocalizer WithCulture(CultureInfo culture) => _localizer;
    }
}
