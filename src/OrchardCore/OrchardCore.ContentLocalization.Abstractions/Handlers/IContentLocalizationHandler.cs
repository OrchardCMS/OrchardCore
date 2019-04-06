using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.ContentLocalization.Handlers
{
    public interface IContentLocalizationHandler
    {
        Task LocalizingAsync(LocalizationContentContext context);
        Task LocalizedAsync(LocalizationContentContext context);
    }
}
