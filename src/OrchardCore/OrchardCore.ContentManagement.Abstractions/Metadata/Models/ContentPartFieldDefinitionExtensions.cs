using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public static class ContentPartFieldDefinitionExtensions
    {
        public static bool IsNamedPart(this ContentPartFieldDefinition fieldDefinition)
            => fieldDefinition.PartDefinition.IsReusable() && fieldDefinition.ContentTypePartDefinition.Name != fieldDefinition.PartDefinition.Name;

        /// <summary>
        /// Returns reusable field wrapper class name.
        /// </summary>
        /// <param name="fieldDefinition">The <see cref="ContentPartFieldDefinition" />.</param>
        /// <returns></returns>
        public static string GetReusableFieldWrapperClassName(this ContentPartFieldDefinition fieldDefinition)
            => $"{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}";

        /// <summary>
        /// Returns field wrapper class name.
        /// </summary>
        /// <param name="fieldDefinition">The <see cref="ContentPartFieldDefinition" />.</param>
        /// <returns></returns>
        public static string GetFieldWrapperClassName(this ContentPartFieldDefinition fieldDefinition)
            =>  $"{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}";


        public static IEnumerable<string> GetFieldWrapperCssClasses(this ContentPartFieldDefinition fieldDefinition, Func<string, string> transform)
        {
            yield return "field-wrapper";
            yield return $"field-wrapper-{transform(fieldDefinition.GetReusableFieldWrapperClassName())}";

            if (fieldDefinition.IsNamedPart())
            {
                yield return $"field-wrapper-{transform(fieldDefinition.GetFieldWrapperClassName())}";
            }
        }
    }
}
