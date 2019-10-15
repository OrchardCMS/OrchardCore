using System;
using System.Collections.Generic;
using OrchardCore.Scripting;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.ContentLocalization
{
    public class ContentLocalizationMethodsProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _syncLocalizations;

        public ContentLocalizationMethodsProvider()
        {
            _syncLocalizations = new GlobalMethod
            {
                Name = "syncLocalizations",
                Method = serviceProvider => (Action<string, object>)(
                  (localizationSet, properties) =>
                    {
                        var contentLocalizationManager = serviceProvider.GetRequiredService<IContentLocalizationManager>();
                        contentLocalizationManager.SyncJson(localizationSet, JObject.FromObject(properties));
                    }
                )
            };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new[] { _syncLocalizations };
        }
    }
}
