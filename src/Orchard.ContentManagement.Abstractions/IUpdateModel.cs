using Microsoft.Extensions.Localization;

namespace Orchard.ContentManagement
{
    public interface IUpdateModel
    {
        bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class;
        void AddModelError(string key, LocalizedString errorMessage);
    }
}