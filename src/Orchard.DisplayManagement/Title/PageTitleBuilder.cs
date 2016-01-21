using Orchard.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.DisplayManagement.Title
{
    public class PageTitleBuilder : IPageTitleBuilder
    {
        private readonly List<PositionalTitlePart> _titleParts;
        private FlatPositionComparer _comparer = new FlatPositionComparer();
        private string _titleSeparator;
        private string _title;

        public PageTitleBuilder()
        {
            _titleParts = new List<PositionalTitlePart>(5);
        }

        public void AddTitlePart(string titlePart, string position)
        {
            _title = null;

            if (!string.IsNullOrEmpty(titlePart))
            {
                _titleParts.Add( new PositionalTitlePart
                    {
                        Value = titlePart,
                        Position = position
                    }
                );
            }
        }

        public void AddTitleParts(string[] titleParts, string position)
        {
            if (titleParts.Length > 0)
            {
                foreach (string titlePart in titleParts)
                {
                    AddTitlePart(titlePart, position);
                }
            }
        }
        
        public string GenerateTitle()
        {
            if(_title != null)
            {
                return _title;
            }

            if (_titleSeparator == null)
            {
                _titleSeparator = " - ";
            }

            _titleParts.Sort(_comparer);

            return _title = _titleParts.Count == 0
                ? String.Empty
                : String.Join(_titleSeparator, _titleParts.Select(x => x.Value).ToArray());
        }
    }

    internal class PositionalTitlePart : IPositioned
    {
        public string Position { get; set; }
        public string Value { get; set; }
    }
}
