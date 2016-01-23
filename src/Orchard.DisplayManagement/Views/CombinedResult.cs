using Orchard.DisplayManagement.Handlers;
using System.Collections.Generic;

namespace Orchard.DisplayManagement.Views
{
    public class CombinedResult<TModel> : DisplayResult<TModel>
    {
        private readonly IEnumerable<DisplayResult<TModel>> _results;

        public CombinedResult(params DisplayResult<TModel>[] results)
        {
            _results = results;
        }

        public CombinedResult(IEnumerable<DisplayResult<TModel>> results)
        {
            _results = results;
        }

        public override void Apply(BuildDisplayContext<TModel> context)
        {
            foreach (var result in _results)
            {
                result.Apply(context);
            }
        }

        public override void Apply(BuildEditorContext<TModel> context)
        {
            foreach (var result in _results)
            {
                result.Apply(context);
            }
        }

        public IEnumerable<DisplayResult<TModel>> GetResults()
        {
            return _results;
        }
    }
}
