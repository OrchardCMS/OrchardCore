using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Localization
{
    public interface ITranslationProvider
    {
        void LoadTranslationsToDictionary(string cultureName, CultureDictionary dictionary);
    }
}
