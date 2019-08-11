using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Implementation;
using StackExchange.Profiling;

namespace OrchardCore.MiniProfiler
{
    public class ShapeStep : IShapeDisplayEvents
    {
        private Dictionary<object, IDisposable> _timings = new Dictionary<object, IDisposable>();

        public Task DisplayedAsync(ShapeDisplayContext context)
        {
            if (_timings.TryGetValue(context, out IDisposable timing))
            {
                _timings.Remove(context);
                timing.Dispose();
            }

            return Task.CompletedTask;
        }

        public Task DisplayingAsync(ShapeDisplayContext context)
        {
            var timing = StackExchange.Profiling.MiniProfiler.Current.Step($"Shape: {context.ShapeMetadata.Type}");
            _timings.Add(context, timing);
            return Task.CompletedTask;
        }

        public Task DisplayingFinalizedAsync(ShapeDisplayContext context)
        {
            if(_timings.TryGetValue(context, out IDisposable timing))
            {
                _timings.Remove(context);
                timing.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}
