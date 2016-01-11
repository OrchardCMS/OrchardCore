using Orchard.ContentManagement.Display.Handlers;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement.Display.Views
{
    public class CombinedResult : DisplayResult
    {
        private readonly IEnumerable<DisplayResult> _results;

        public CombinedResult(params DisplayResult[] results)
        {
            _results = results;
        }

        public CombinedResult(IEnumerable<DisplayResult> results)
        {
            _results = results;
        }

        public override void Apply(BuildDisplayContext context)
        {
            foreach (var result in _results)
            {
                result.Apply(context);
            }
        }

        public override void Apply(BuildEditorContext context)
        {
            foreach (var result in _results)
            {
                result.Apply(context);
            }
        }

        public IEnumerable<DisplayResult> GetResults()
        {
            return _results;
        }
    }
}
