using Orchard.DisplayManagement.Handlers;
using System.Collections.Generic;

namespace Orchard.DisplayManagement.Views
{
    public class CombinedResult : IDisplayResult
    {
        private readonly IEnumerable<IDisplayResult> _results;

        public CombinedResult(params IDisplayResult[] results)
        {
            _results = results;
        }

        public CombinedResult(IEnumerable<IDisplayResult> results)
        {
            _results = results;
        }

        public void Apply(BuildDisplayContext context)
        {
            foreach (var result in _results)
            {
                result.Apply(context);
            }
        }

        public void Apply(BuildEditorContext context)
        {
            foreach (var result in _results)
            {
                result.Apply(context);
            }
        }

        public IEnumerable<IDisplayResult> GetResults()
        {
            return _results;
        }
    }
}
