using System.Collections.Generic;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services {
    public interface ICultureManager {
        IEnumerable<string> ListCultures();
        void AddCulture(string cultureName);
        void DeleteCulture(string cultureName);
        //string GetCurrentCulture(HttpContextBase requestContext);
        ICulture GetCultureById(int id);
        ICulture GetCultureByName(string cultureName);
        string GetSiteCulture();
        bool IsValidCulture(string cultureName);
    }
}
