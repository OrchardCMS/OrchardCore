using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Microsoft.Extensions.Logging;
using YesSql.Core.Services;
using System.Threading.Tasks;

namespace Orchard.Core.Settings.Metadata
{
    public class ContentDefinitionManager : IContentDefinitionManager {
        private readonly ISession _session;
        private readonly ISettingsFormatter _settingsFormatter;
        private readonly ILogger _logger;

        private ContentDefinition ContentDefinition;

        public ContentDefinitionManager(
            ISession session,
            ISettingsFormatter settingsFormatter,
            ILoggerFactory loggerFactory) {
            _session = session;
            _settingsFormatter = settingsFormatter;
            _logger = loggerFactory.CreateLogger<ContentDefinitionManager>();
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions() {
            LoadContentDefinition().Wait();
            return ContentDefinition.ContentTypeDefinitions.Values;
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions() {
            LoadContentDefinition().Wait();
            return ContentDefinition.ContentPartDefinitions.Values;
        }

        public IEnumerable<ContentFieldDefinition> ListFieldDefinitions() {
            LoadContentDefinition().Wait();
            return ContentDefinition.ContentFieldDefinitions.Values;
        }

        public ContentTypeDefinition GetTypeDefinition(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return null;
            }

            LoadContentDefinition().Wait();

            if (ContentDefinition.ContentTypeDefinitions.ContainsKey(name)) {
                return ContentDefinition.ContentTypeDefinitions[name];
            }

            return null;
        }

        public ContentPartDefinition GetPartDefinition(string name) {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            LoadContentDefinition().Wait();

            if (ContentDefinition.ContentPartDefinitions.ContainsKey(name))
            {
                return ContentDefinition.ContentPartDefinitions[name];
            }

            return null;
        }

        public void DeleteTypeDefinition(string name) {
            LoadContentDefinition().Wait();

            if (ContentDefinition.ContentTypeDefinitions.ContainsKey(name))
            {
                ContentDefinition.ContentTypeDefinitions.Remove(name);
            }
        }

        public void DeletePartDefinition(string name) {
            LoadContentDefinition().Wait();

            // Remove parts from current types
            var typesWithPart = ListTypeDefinitions().Where(typeDefinition => typeDefinition.Parts.Any(part => part.PartDefinition.Name == name));

            foreach (var typeDefinition in typesWithPart) {
                this.AlterTypeDefinition(typeDefinition.Name, builder => builder.RemovePart(name));
            }

            if (ContentDefinition.ContentPartDefinitions.ContainsKey(name))
            {
                ContentDefinition.ContentPartDefinitions.Remove(name);
            }
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition) {
            ContentDefinition.ContentTypeDefinitions.Add(contentTypeDefinition.Name, contentTypeDefinition);
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition) {
            ContentDefinition.ContentPartDefinitions.Add(contentPartDefinition.Name, contentPartDefinition);
        }

        private async Task LoadContentDefinition()
        {
            if(ContentDefinition == null)
            {
                ContentDefinition = await _session
                    .QueryAsync<ContentDefinition>()
                    .FirstOrDefault()
                    ;
            }
        }
    }
}