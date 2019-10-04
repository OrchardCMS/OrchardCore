using System.Collections.Generic;

namespace OrchardCore.Localization.Models
{
    public class TranslationsDocument
    {
        public int Id { get; set; }
        public List<Translation> Translations { get; } = new List<Translation>();
    }
}