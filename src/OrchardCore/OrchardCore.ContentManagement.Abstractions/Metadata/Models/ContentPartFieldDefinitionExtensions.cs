namespace OrchardCore.ContentManagement.Metadata.Models
{
    public static class ContentPartFieldDefinitionExtensions
    {
        public static bool IsNamedPart(this ContentPartFieldDefinition fieldDefinition)
            => fieldDefinition.ContentTypePartDefinition.Name != fieldDefinition.PartDefinition.Name;

        /// <summary>
        /// Returns full field name, if field belongs to named part technical name is used.
        /// </summary>
        /// <param name="fieldDefinition"></param>
        /// <returns></returns>
        public static string GetFullName(this ContentPartFieldDefinition fieldDefinition)
            => fieldDefinition.IsNamedPart()
                ? $"{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}"
                : $"{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}";
    }
}
