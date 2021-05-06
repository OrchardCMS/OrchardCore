using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Flows.Models;

namespace OrchardCore.Forms.Helpers
{
    public class ValidationRuleHelpers
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;

        public ValidationRuleHelpers(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ITypeActivatorFactory<ContentPart> contentPartFactory)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _contentPartFactory = contentPartFactory;
        }
        public static bool ValidateLength(int len, string option)
        {
            try
            {
                var min = 0;
                var max = Int32.MaxValue;
                var obj = JToken.Parse(option);
                Int32.TryParse(obj["max"]?.ToString(), out max);
                Int32.TryParse(obj["min"]?.ToString(), out min);
                if (len >= min && (max == 0 || len <= max)) return true;
                return false;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        public static bool ValidateIs<T>(string value)
        {
            if (value == null) return false;
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            try
            {
                var result = (T)converter.ConvertFromString(value.ToString());
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool CompareDatetime(string input, string option, string symbol)
        {
            var originResult = DateTime.TryParse(input, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var originDate);
            var compareResult = DateTime.TryParse(option, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var compareDate);
            if (symbol == ">") return originResult && compareResult && originDate > compareDate;
            return originResult && compareResult && originDate < compareDate;
        }

        public async Task<List<FlowPart>> GeFlowPartFromContentItemId(string contentItemId)
        {
            List<FlowPart> flowParts = new List<FlowPart>();

            var contentItem = await _contentManager.GetAsync(contentItemId);
            if (contentItem == null) return null;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            if (contentTypeDefinition == null) return null;

            foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
            {
                //var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                //var contentType = contentTypePartDefinition.ContentTypeDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);
                var flowPart = contentItem.As<FlowPart>();
                if (flowPart == null) continue;

                foreach (var widget in flowPart.Widgets)
                {
                    var formPartTypeDefinition = _contentDefinitionManager.GetTypeDefinition(widget.ContentType);

                    foreach (var formWidgetTypeDefinition in formPartTypeDefinition.Parts)
                    {
                        //var formPartName = formWidgetTypeDefinition.Name;
                        var formPartTypeName = formWidgetTypeDefinition.PartDefinition.Name;
                        //var formContentType = formWidgetTypeDefinition.ContentTypeDefinition.Name;
                        var formPartActivator = _contentPartFactory.GetTypeActivator(formPartTypeName);
                        var formFlowPart = widget.As<FlowPart>();
                        if (formFlowPart == null) continue;

                        flowParts.Add(formFlowPart);
                    }
                }
            }
            return flowParts;
        }
    }
}
