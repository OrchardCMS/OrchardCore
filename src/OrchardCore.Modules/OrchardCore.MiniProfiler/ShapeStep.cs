using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Implementation;
using StackExchange.Profiling;

namespace OrchardCore.MiniProfiler
{
    public class ShapeStep : IShapeDisplayEvents
    {
        private Stack<IDisposable> _timings = new Stack<IDisposable>();

        public Task DisplayedAsync(ShapeDisplayContext context)
        {
            return Task.CompletedTask;
        }

        public Task DisplayingAsync(ShapeDisplayContext context)
        {
            var timing = StackExchange.Profiling.MiniProfiler.Current.Step($"Shape: {context.ShapeMetadata.Type}");
            _timings.Push(timing);
            return Task.CompletedTask;
        }

        public Task DisplayingFinalizedAsync(ShapeDisplayContext context)
        {
            var timing = _timings.Pop();

            timing?.Dispose();

            return Task.CompletedTask;
        }
    }
}
