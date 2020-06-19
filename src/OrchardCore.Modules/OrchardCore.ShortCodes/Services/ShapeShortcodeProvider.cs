using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Shortcodes;
using OrchardCore.ShortCodes;
using System.Collections;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ShortCodes.Services
{
    public class ShapeShortcodeProvider : IShortcodeProvider, IEnumerable
    {
        private readonly IShapeFactory _shapeFactory;
        private readonly IDisplayHelper _displayHelper;
        private readonly HtmlEncoder _htmlEncoder;
        private Dictionary<string, string> _shortcodeIdentifiers;

        public ShapeShortcodeProvider(
            IShapeFactory shapeFactory,
            IDisplayHelper displayHelper,
            HtmlEncoder htmlEncoder,
            IShapeTableManager shapeTableManager,
            IThemeManager themeManager
            )
        {
            _shapeFactory = shapeFactory;
            _htmlEncoder = htmlEncoder;
            _displayHelper = displayHelper;

            var theme = themeManager.GetThemeAsync().GetAwaiter().GetResult();
            var shapeTable = shapeTableManager.GetShapeTable(theme?.Id);
            // TODO handle _ as a potential display modifier;
            var shortcodeShapes = shapeTable?.Bindings.Keys.Where(x => x.StartsWith("Shortcode", StringComparison.OrdinalIgnoreCase));
            _shortcodeIdentifiers = shortcodeShapes?.ToDictionary(key =>
                {
                    var delimiterIndex = key.IndexOf("__", StringComparison.Ordinal);
                    return key.Substring(delimiterIndex + 2, key.Length - delimiterIndex - 2);
                }, value => value);

            _shortcodeIdentifiers ??= new Dictionary<string, string>();
        }

        public async ValueTask<string> EvaluateAsync(string identifier, Shortcodes.Arguments arguments, string content)
        {
            if (_shortcodeIdentifiers.TryGetValue(identifier, out var shapeName))
            {
                var shape = await _shapeFactory.CreateAsync(shapeName, OrchardCore.DisplayManagement.Arguments.From(
                    new
                    {
                        Content = content,
                        Arguments = arguments
                    }));
                var htmlContent = await _displayHelper.ShapeExecuteAsync(shape);

                return GetString(htmlContent);
            }

            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return _shortcodeIdentifiers.Keys.GetEnumerator();
        }

        public string GetString(IHtmlContent content)
        {
            using (var writer = new StringWriter())
            {
                content.WriteTo(writer, _htmlEncoder);
                return writer.ToString();
            }
        }
    }
}
