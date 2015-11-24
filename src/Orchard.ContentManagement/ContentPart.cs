using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement.MetaData.Models;
using System.Runtime.Serialization;
#if DNXCORE50
using System.Reflection;
#endif

namespace Orchard.ContentManagement
{
    public class ContentPart : IContent
    {
        [IgnoreDataMember]
        public virtual ContentItem ContentItem { get; set; }

        /// <summary>
        /// The ContentItem's identifier.
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        [IgnoreDataMember]
        public int ContentItemId => ContentItem.ContentItemId;

        [IgnoreDataMember]
        public ContentTypePartDefinition TypePartDefinition { get; set; }
        public SettingsDictionary Settings { get; }

        public IList<ContentField> Fields { get; set; } = new List<ContentField>();

        public bool Has(Type fieldType, string fieldName)
        {
            return Fields.Any(field => field.Name == fieldName && fieldType.IsInstanceOfType(field));
        }

        public ContentField Get(Type fieldType, string fieldName)
        {
            return Fields.FirstOrDefault(field => field.Name == fieldName && fieldType.IsInstanceOfType(field));
        }

        public void Weld(ContentField field)
        {
            Fields.Add(field);
        }
    }
}