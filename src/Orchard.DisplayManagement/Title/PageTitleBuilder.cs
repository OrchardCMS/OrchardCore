using Orchard.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Title
{
    public class PageTitleBuilder : IPageTitleBuilder
    {
        private readonly List<PositionalTitlePart> _titleParts;
        private string _titleSeparator;
        private string _title;

        public PageTitleBuilder()
        {
            _titleParts = new List<PositionalTitlePart>(5);
        }

        public void AddSegment(string titlePart, string position)
        {
            _title = null;

            if (!string.IsNullOrEmpty(titlePart))
            {
                _titleParts.Add(new PositionalTitlePart
                {
                    Value = titlePart,
                    Position = position
                });
            }
        }

        public void AddSegments(string[] titleParts, string position)
        {
            if (titleParts.Length > 0)
            {
                foreach (string titlePart in titleParts)
                {
                    AddSegment(titlePart, position);
                }
            }
        }

        public string GenerateTitle()
        {
            if (_title != null)
            {
                return _title;
            }

            if (_titleSeparator == null)
            {
                _titleSeparator = " - ";
            }

            _titleParts.Sort(FlatPositionComparer.Instance);

            return _title = _titleParts.Count == 0
                ? String.Empty
                : String.Join(_titleSeparator, _titleParts.Select(x => x.Value).ToArray());
        }

        public void Clear()
        {
            _titleParts.Clear();
        }
    }

    internal class PositionalTitlePart : IPositioned
    {
        public string Position { get; set; }
        public string Value { get; set; }
    }
}
