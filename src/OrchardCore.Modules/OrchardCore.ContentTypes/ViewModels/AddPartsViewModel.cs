using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentTypes.ViewModels;

public class AddPartsViewModel
{
    public AddPartsViewModel()
    {
        PartSelections = [];
    }

    public EditTypeViewModel Type { get; set; }
    public List<PartSelectionViewModel> PartSelections { get; set; }

    /// <summary>
    /// Gets or sets the editor of the parts to add, built by the display drivers of <see cref="AddPartsViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}

public class AddReusablePartViewModel
{
    public AddReusablePartViewModel()
    {
        PartSelections = [];
    }

    public EditTypeViewModel Type { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public List<PartSelectionViewModel> PartSelections { get; set; }
    public string SelectedPartName { get; set; }

    /// <summary>
    /// Gets or sets the editor of the named part to add, built by the display drivers of <see cref="AddReusablePartViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}

public class PartSelectionViewModel
{
    public string PartName { get; set; }
    public string PartDisplayName { get; set; }
    public string PartDescription { get; set; }
    public bool IsSelected { get; set; }
}
