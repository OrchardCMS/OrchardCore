using OrchardCore.DataLocalization.Models;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Reads and writes UI overrides in a file format used for import and export.</summary>
internal interface IUITranslationsFormat
{
    /// <summary>Gets the file extension identifying the format, without the leading dot.</summary>
    string Extension { get; }

    /// <summary>Reads the translations of a file. Entries without translations have empty values.</summary>
    /// <param name="reader">The file reader.</param>
    IEnumerable<UITranslation> Read(TextReader reader);

    /// <summary>Writes translations, retaining their contexts and plural source identifiers.</summary>
    /// <param name="writer">The file writer.</param>
    /// <param name="culture">The culture of the translations.</param>
    /// <param name="translations">The translations.</param>
    void Write(TextWriter writer, string culture, IEnumerable<UITranslation> translations);
}
