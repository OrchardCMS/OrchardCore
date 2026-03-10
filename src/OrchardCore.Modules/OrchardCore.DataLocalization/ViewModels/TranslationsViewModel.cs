using OrchardCore.DataLocalization.Models;

namespace OrchardCore.DataLocalization.ViewModels;

public class TranslationsViewModel
{
    public string Key { get; set; }

    public IEnumerable<Translation> Translations { get; set; } = [];
}
