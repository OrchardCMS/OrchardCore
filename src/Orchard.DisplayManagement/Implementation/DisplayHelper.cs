using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Html;
using System.IO;
using Microsoft.Extensions.WebEncoders;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Implementation
{
    public class DisplayHelper : DynamicObject
    {
        private readonly IHtmlDisplay _displayManager;
        private readonly IShapeFactory _shapeFactory;

        public DisplayHelper(
            IHtmlDisplay displayManager,
            IShapeFactory shapeFactory,
            ViewContext viewContext)
        {
            _displayManager = displayManager;
            _shapeFactory = shapeFactory;
            ViewContext = viewContext;
        }

        public ViewContext ViewContext { get; set; }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = InvokeAsync(null, Arguments.From(args, binder.CallInfo.ArgumentNames)).Result;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = InvokeAsync(binder.Name, Arguments.From(args, binder.CallInfo.ArgumentNames)).Result;
            return true;
        }

        public async Task<object> InvokeAsync(string name, INamedEnumerable<object> parameters)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return await ShapeTypeExecuteAsync(name, parameters);
            }

            if (parameters.Positional.Count() == 1)
            {
                return await ShapeExecuteAsync(parameters.Positional.First());
            }

            if (parameters.Positional.Any())
            {
                return new Combined(await ShapeExecuteAsync(parameters.Positional));
            }

            // zero args - no display to execute
            return null;
        }

        // TODO: Replace with HtmlContentBuilder once available in MVC 6 rc2
        public class Combined : IHtmlContent
        {
            private readonly IEnumerable<IHtmlContent> _fragments;

            public Combined(IEnumerable<IHtmlContent> fragments)
            {
                _fragments = fragments;
            }

            public void WriteTo(TextWriter writer, IHtmlEncoder encoder)
            {
                foreach (var fragment in _fragments)
                {
                    fragment.WriteTo(writer, encoder);
                }
            }
        }

        private Task<IHtmlContent> ShapeTypeExecuteAsync(string name, INamedEnumerable<object> parameters)
        {
            var shape = _shapeFactory.Create(name, parameters);
            return ShapeExecuteAsync(shape);
        }

        public Task<IHtmlContent> ShapeExecuteAsync(Shape shape)
        {
            // disambiguates the call to ShapeExecute(object) as Shape also implements IEnumerable
            return ShapeExecuteAsync((object)shape);
        }

        public async Task<IHtmlContent> ShapeExecuteAsync(object shape)
        {
            if (shape == null)
            {
                return new HtmlString(string.Empty);
            }

            var context = new DisplayContext { Display = this, Value = shape, ViewContext = ViewContext };
            return await _displayManager.ExecuteAsync(context);
        }

        public async Task<IEnumerable<IHtmlContent>> ShapeExecuteAsync(IEnumerable<object> shapes)
        {
            var result = new List<IHtmlContent>();
            foreach (var shape in shapes)
            {
                result.Add(await ShapeExecuteAsync(shape));
            }

            return result;
        }
    }
}