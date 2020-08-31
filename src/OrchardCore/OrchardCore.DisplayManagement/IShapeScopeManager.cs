using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement
{
    public interface IShapeScopeManager
    {
        void EnterScope(ShapeScopeContext context);
        void ExitScope();

        void AddShape(IShape shape);
        void AddSlot(string slot, dynamic content);
        dynamic GetSlot(string slot);
        IShape GetCurrentShape();
    }

    public class ShapeScopeContext
    {
        private IShape _shape;
        private Dictionary<string, dynamic> _slots;

        public ShapeScopeContext AddShape(IShape shape)
        {
            _shape = shape;

            return this;
        }

        public ShapeScopeContext AddSlot(string slot, dynamic content)
        {
            _slots ??= new Dictionary<string, dynamic>();
            _slots[slot] = content;

            return this;
        }

        public dynamic GetSlot(string slot)
        {
            _slots ??= new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);

            if (_slots.TryGetValue(slot, out var result))
            {
                return result;
            }

            return HtmlString.Empty;
        }

        public IShape GetCurrentShape()
        {
            return _shape;
        }
    }
}
