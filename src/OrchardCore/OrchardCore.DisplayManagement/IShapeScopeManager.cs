using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement
{
    public interface IShapeScopeManager
    {
        void EnterScope(ShapeScopeContext context);
        void ExitScope();

        void AddShape(IShape shape);
        void AddSlot(string slot, dynamic content);
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
    }
}
