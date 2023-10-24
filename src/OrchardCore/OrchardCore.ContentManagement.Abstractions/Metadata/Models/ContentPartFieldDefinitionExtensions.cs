using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public static class ContentPartFieldDefinitionExtensions
    {
        public static bool IsNamedPart(this ContentPartFieldDefinition fieldDefinition)
            => fieldDefinition.PartDefinition.IsReusable() && fieldDefinition.ContentTypePartDefinition.Name != fieldDefinition.PartDefinition.Name;

        /// <summary>
        /// Returns full field name.
        /// </summary>
        /// <param name="fieldDefinition">The <see cref="ContentPartFieldDefinition" />.</param>
        /// <returns></returns>
        public static string GetFullName(this ContentPartFieldDefinition fieldDefinition)
            => $"{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}";

        /// <summary>
        /// Returns unique full field name.
        /// </summary>
        /// <param name="fieldDefinition">The <see cref="ContentPartFieldDefinition" />.</param>
        /// <returns></returns>
        public static string GetUniqueFullName(this ContentPartFieldDefinition fieldDefinition)
            =>  $"{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}";


        public static IEnumerable<string> GetAdditionalClasses(this ContentPartFieldDefinition fieldDefinition, string prefix, Func<string, string> transform)
        {
            yield return prefix;
            yield return $"{prefix}-{transform(fieldDefinition.GetFullName())}";

            if (fieldDefinition.IsNamedPart())
            {
                yield return $"{prefix}-{transform(fieldDefinition.GetUniqueFullName())}";
            }
        }
    }
}
