using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System;

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
            try
            {
                result = InvokeAsync(null, Arguments.From(args, binder.CallInfo.ArgumentNames)).Result;
            }
            catch (AggregateException ae)
            {
                // Unwrap the aggregate exception
                throw ae.GetBaseException();
            }

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                result = InvokeAsync(binder.Name, Arguments.From(args, binder.CallInfo.ArgumentNames)).Result;
            }
            catch (AggregateException ae)
            {
                // Unwrap the aggregate exception
                throw ae.GetBaseException();
            }

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
                var htmlContents = await ShapeExecuteAsync(parameters.Positional);
                var htmlContentBuilder = new HtmlContentBuilder();
                foreach (var htmlContent in htmlContents)
                {
                    htmlContentBuilder.AppendHtml(htmlContent);
                }
            }

            // zero args - no display to execute
            return null;
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