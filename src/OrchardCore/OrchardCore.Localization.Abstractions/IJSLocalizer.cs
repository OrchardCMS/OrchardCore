using System.Collections.Generic;

namespace OrchardCore.Localization;
public interface IJSLocalizer {
    public IDictionary<string, string> GetLocalizations(string[] groups);
}
