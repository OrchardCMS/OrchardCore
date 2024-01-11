using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.DisplayManagement.Views
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

        public async Task ApplyAsync(BuildDisplayContext context)
        {
            foreach (var result in _results)
            {
                await result.ApplyAsync(context);
            }
        }

        public async Task ApplyAsync(BuildEditorContext context)
        {
            foreach (var result in _results)
            {
                await result.ApplyAsync(context);
            }
        }

        public IEnumerable<IDisplayResult> GetResults()
        {
            return _results;
        }
    }
}
