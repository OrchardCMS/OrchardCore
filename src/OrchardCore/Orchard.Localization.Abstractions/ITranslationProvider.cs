using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Localization.Abstractions
{
    public interface ITranslationProvider
    {
        void LoadTranslationsToDictionary(string cultureName, CultureDictionary dictionary);
    }
}
