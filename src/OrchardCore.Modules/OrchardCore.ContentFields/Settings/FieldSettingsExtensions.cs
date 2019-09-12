using System;
using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentFields.Settings
{
    public static class FieldSettingsExtensions
    {
        //TODO maybe just remove now.
        [Obsolete("Please migrate to use Hint<T>. This will be removed in future versions.")]
        public static ContentPartFieldDefinitionBuilder Hint(this ContentPartFieldDefinitionBuilder builder, string hint)
        {
            return builder.WithSetting(nameof(Hint), hint);
        }

        //HACK. do better. settings hint base class? Probably
        public static ContentPartFieldDefinitionBuilder Hint<T>(this ContentPartFieldDefinitionBuilder builder, string hint)
            where T: class, new()
        {
            return builder.MergeSettings<T>((new T() as dynamic).Hint = hint);
        }
    }
}
