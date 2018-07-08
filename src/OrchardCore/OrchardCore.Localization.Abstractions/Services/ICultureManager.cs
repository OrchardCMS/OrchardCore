using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services {
    public interface ICultureManager {
        IEnumerable<Culture> ListCultures();
        void AddCulture(string cultureName);
        void DeleteCulture(string cultureName);
        string GetCurrentCulture();
        string GetSiteCulture();
        bool IsValidCulture(string cultureName);
        bool CultureExist(string cultureName);
    }
}
