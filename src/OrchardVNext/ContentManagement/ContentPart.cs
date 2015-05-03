using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc;
using OrchardVNext.ContentManagement.MetaData.Models;

namespace OrchardVNext.ContentManagement {
    public class ContentPart : IContent {
        private readonly IList<ContentField> _fields;

        public ContentPart() {
            _fields = new List<ContentField>();
        }

        public virtual ContentItem ContentItem { get; set; }

        /// <summary>
        /// The ContentItem's identifier.
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public int Id => ContentItem.Id;

        public ContentTypeDefinition TypeDefinition => ContentItem.TypeDefinition;
        public ContentTypePartDefinition TypePartDefinition { get; set; }
        public ContentPartDefinition PartDefinition => TypePartDefinition.PartDefinition;
        public SettingsDictionary Settings => TypePartDefinition.Settings;

        public IEnumerable<ContentField> Fields => _fields;


        public bool Has(Type fieldType, string fieldName) {
            return _fields.Any(field => field.Name == fieldName && fieldType.IsInstanceOfType(field));
        }

        public ContentField Get(Type fieldType, string fieldName) {
            return _fields.FirstOrDefault(field => field.Name == fieldName && fieldType.IsInstanceOfType(field));
        }

        public void Weld(ContentField field) {
            _fields.Add(field);
        }

        public T Retrieve<T>(string fieldName) {
            return InfosetHelper.Retrieve<T>(this, fieldName);
        }

        public T RetrieveVersioned<T>(string fieldName) {
            return this.Retrieve<T>(fieldName, true);
        }

        public virtual void Store<T>(string fieldName, T value) {
            InfosetHelper.Store(this, fieldName, value);
        }

        public virtual void StoreVersioned<T>(string fieldName, T value) {
            this.Store(fieldName, value, true);
        }
    }
}