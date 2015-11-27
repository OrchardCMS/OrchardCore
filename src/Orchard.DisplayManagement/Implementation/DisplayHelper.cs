using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Html.Abstractions;
using System.IO;
using Microsoft.Extensions.WebEncoders;

namespace Orchard.DisplayManagement.Implementation
{
    public class DisplayHelper : DynamicObject
    {
        private readonly IDisplayManager _displayManager;
        private readonly IShapeFactory _shapeFactory;

        public DisplayHelper(
            IDisplayManager displayManager,
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
            result = Invoke(null, Arguments.From(args, binder.CallInfo.ArgumentNames));
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = Invoke(binder.Name, Arguments.From(args, binder.CallInfo.ArgumentNames));
            return true;
        }

        public object Invoke(string name, INamedEnumerable<object> parameters)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return ShapeTypeExecute(name, parameters);
            }

            if (parameters.Positional.Count() == 1)
            {
                return ShapeExecute(parameters.Positional.Single());
            }

            if (parameters.Positional.Any())
            {
                return new Combined(ShapeExecute(parameters.Positional));
            }

            // zero args - no display to execute
            return null;
        }

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
                    writer.Write(fragment.ToString());
                }
            }
        }

        private IHtmlContent ShapeTypeExecute(string name, INamedEnumerable<object> parameters)
        {
            var shape = _shapeFactory.Create(name, parameters);
            return ShapeExecute(shape);
        }

        public IHtmlContent ShapeExecute(Shape shape)
        {
            // disambiguates the call to ShapeExecute(object) as Shape also implements IEnumerable
            return ShapeExecute((object)shape);
        }

        public IHtmlContent ShapeExecute(object shape)
        {
            if (shape == null)
            {
                return new HtmlString(string.Empty);
            }

            var context = new DisplayContext { Display = this, Value = shape, ViewContext = ViewContext };
            return _displayManager.Execute(context);
        }

        public IEnumerable<IHtmlContent> ShapeExecute(IEnumerable<object> shapes)
        {
            return shapes.Select(ShapeExecute).ToArray();
        }
    }
}