using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Seo
{
    public class SeoPageTitleBuilder : IPageTitleBuilder
    {
        private readonly List<PositionalTitlePart> _titleParts;
        private IHtmlContent _title;
        private IHtmlContent _customTitle;

        public SeoPageTitleBuilder()
        {
            _titleParts = new List<PositionalTitlePart>(5);
        }

        public void SetTitle(string title)
        {
            _customTitle = new HtmlString(title);
        }

        public void AddSegment(IHtmlContent titlePart, string position)
        {
            _title = null;

            _titleParts.Add(new PositionalTitlePart
            {
                Value = titlePart,
                Position = position
            });
        }

        public void AddSegments(IEnumerable<IHtmlContent> titleParts, string position)
        {
            foreach (var titlePart in titleParts)
            {
                AddSegment(titlePart, position);
            }
        }

        public IHtmlContent GenerateTitle(IHtmlContent separator)
        {
            if (_customTitle != null)
            {
                return _customTitle;
            }
            if (_title != null)
            {
                return _title;
            }

            if (separator == null)
            {
                separator = new HtmlString(" - ");
            }

            _titleParts.Sort(FlatPositionComparer.Instance);

            var htmlContentBuilder = new HtmlContentBuilder();

            if (_titleParts.Count == 0)
            {
                return HtmlString.Empty;
            }

            for (var i = 0; i < _titleParts.Count; i++)
            {
                htmlContentBuilder.AppendHtml(_titleParts[i].Value);

                if (i < _titleParts.Count - 1)
                {
                    htmlContentBuilder.AppendHtml(separator);
                }
            }

            _title = htmlContentBuilder;

            return _title;
        }

        public void Clear()
        {
            _titleParts.Clear();
        }
    }

    internal class PositionalTitlePart : IPositioned
    {
        public string Position { get; set; }
        public IHtmlContent Value { get; set; }
    }
}
