namespace OrchardCore.ContentManagement.Metadata.Models
{
    public static class ContentPartFieldDefinitionExtensions
    {
        public static bool IsNamedPart(this ContentPartFieldDefinition fieldDefinition)
        {
            return fieldDefinition.ContentTypePartDefinition.Name != fieldDefinition.PartDefinition.Name;
        }

        public static string UniqueNameRegularPart(this ContentPartFieldDefinition fieldDefinition)
        {
            return $"{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}";
        }

        public static string UniqueNameNamedPart(this ContentPartFieldDefinition fieldDefinition)
        {
            return $"{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}";
        }

        public static string UniqueName(this ContentPartFieldDefinition fieldDefinition)
        {
            return fieldDefinition.IsNamedPart() ? fieldDefinition.UniqueNameNamedPart() : fieldDefinition.UniqueNameRegularPart();
        }
    }
}
