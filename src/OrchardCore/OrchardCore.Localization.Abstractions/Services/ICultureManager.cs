using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services {
    public interface ICultureManager {
        IEnumerable<CultureRecord> ListCultures();
        void AddCulture(string cultureName);
        void DeleteCulture(string cultureName);
        string GetCurrentCulture(HttpContext requestContext);
        CultureRecord GetCultureById(int id);
        CultureRecord GetCultureByName(string cultureName);
        string GetSiteCulture();
        bool IsValidCulture(string cultureName);
    }
}
