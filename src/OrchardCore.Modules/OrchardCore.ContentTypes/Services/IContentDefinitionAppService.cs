using OrchardCore.ContentTypes.ViewModels;

namespace OrchardCore.ContentTypes.Services;

// This service for UI layering, not for core content definition management.
public interface IContentDefinitionViewModelService
{
    Task<IEnumerable<EditTypeViewModel>> LoadTypesAsync();

    Task<IEnumerable<EditTypeViewModel>> GetTypesAsync();

    Task<EditTypeViewModel> LoadTypeAsync(string name);

    Task<EditTypeViewModel> GetTypeAsync(string name);

    Task<IEnumerable<EditPartViewModel>> LoadPartsAsync(bool metadataPartsOnly);

    Task<IEnumerable<EditPartViewModel>> GetPartsAsync(bool metadataPartsOnly);

    Task<EditPartViewModel> LoadPartAsync(string name);

    Task<EditPartViewModel> GetPartAsync(string name);

    Task<EditPartViewModel> AddPartAsync(CreatePartViewModel partViewModel);

    Task AlterFieldAsync(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel);

    Task AlterTypePartAsync(EditTypePartViewModel partViewModel);
}
