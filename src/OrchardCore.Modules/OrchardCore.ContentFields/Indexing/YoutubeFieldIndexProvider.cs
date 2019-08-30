using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql.Indexes;

namespace OrchardCore.ContentFields.Indexing
{
    public class YoutubeFieldIndex : ContentFieldIndex
    {
        public string EmbeddedAddress { get; set; }
        public string RawAddress { get; set; }
    }

    public class YoutubeFieldIndexProvider : ContentFieldIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();
        private IContentDefinitionManager _contentDefinitionManager;

        public YoutubeFieldIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<YoutubeFieldIndex>()
                .Map(contentItem =>
                {
                    // Can we safely ignore this content item?
                    if (_ignoredTypes.Contains(contentItem.ContentType))
                    {
                        return null;
                    }

                    if (!contentItem.Latest && !contentItem.Published) {
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager = _contentDefinitionManager ?? _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Search for Time fields
                    var fieldDefinitions = _contentDefinitionManager
                        .GetTypeDefinition(contentItem.ContentType)
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(YoutubeField)))
                        .ToArray();

                    var results = new List<YoutubeFieldIndex>();

                    foreach (var fieldDefinition in fieldDefinitions)
                    {
                        var jPart = (JObject)contentItem.Content[fieldDefinition.PartDefinition.Name];

                        if (jPart == null)
                        {
                            continue;
                        }

                        var jField = (JObject)jPart[fieldDefinition.Name];

                        if (jField == null)
                        {
                            continue;
                        }

                        var field = jField.ToObject<YoutubeField>();

                        if (!String.IsNullOrEmpty(field.EmbeddedAddress))
                        {
                            results.Add(new YoutubeFieldIndex
                            {
                                Latest = contentItem.Latest,
                                Published = contentItem.Published,
                                ContentItemId = contentItem.ContentItemId,
                                ContentType = contentItem.ContentType,
                                ContentPart = fieldDefinition.PartDefinition.Name,
                                ContentField = fieldDefinition.Name,
                                EmbeddedAddress = field.EmbeddedAddress.Substring(0, Math.Min(field.EmbeddedAddress.Length, 4000)),
                                RawAddress = field.RawAddress.Substring(0, Math.Min(field.RawAddress.Length, 4000)),
                            });
                        }
                    }

                    return results;
                });
        }
    }
}