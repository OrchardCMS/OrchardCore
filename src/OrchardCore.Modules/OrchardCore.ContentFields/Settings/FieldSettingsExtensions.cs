using System;
using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentFields.Settings
{
    public static class FieldSettingsExtensions
    {
        [Obsolete("Please migrate to use WithSettings<T>. This will be removed in future versions.")]
        public static ContentPartFieldDefinitionBuilder Hint(this ContentPartFieldDefinitionBuilder builder, string hint)
        {
            return builder.WithSetting(nameof(Hint), hint);
        }
    }
}
