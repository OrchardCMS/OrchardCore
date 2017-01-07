using System;
using Glimpse;
using Glimpse.Common;
using Glimpse.Internal;

namespace Orchard.Glimpse
{
    public class OrchardGlimpseContextAccessor : IGlimpseContextAccessor
    {
        private readonly IContextData<MessageContext> _context;
        public OrchardGlimpseContextAccessor(IContextData<MessageContext> context)
        {
            _context = context;
        }

        public Guid RequestId { get { return _context.Value == null ? Guid.NewGuid() : _context.Value.Id; } }
    }
}