using System.ComponentModel.DataAnnotations;

namespace OrchardCore.ContentManagement;

public interface IContentImportErrorProvider
{
    ValueTask<string> GetDetailsAsync(ContentItem importingItem, IReadOnlyList<ValidationResult> errors);
}
