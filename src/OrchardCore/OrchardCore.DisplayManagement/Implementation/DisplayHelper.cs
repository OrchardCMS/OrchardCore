using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Implementation
{
    public class DisplayHelper : DynamicObject, IDisplayHelper
    {
        private readonly IHtmlDisplay _htmlDisplay;
        private readonly IShapeFactory _shapeFactory;
        private readonly IServiceProvider _serviceProvider;

        public DisplayHelper(
            IHtmlDisplay htmlDisplay,
            IShapeFactory shapeFactory,
            IServiceProvider serviceProvider)
        {
            _htmlDisplay = htmlDisplay;
            _shapeFactory = shapeFactory;
            _serviceProvider = serviceProvider;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = InvokeAsync(null, Arguments.From(args, binder.CallInfo.ArgumentNames));

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = InvokeAsync(binder.Name, Arguments.From(args, binder.CallInfo.ArgumentNames));

            return true;
        }

        public Task<IHtmlContent> InvokeAsync(string name, INamedEnumerable<object> parameters)
        {
            if (!String.IsNullOrEmpty(name))
            {
                return ShapeTypeExecuteAsync(name, parameters);
            }

            if (parameters.Positional.Count == 1)
            {
                return ShapeExecuteAsync(parameters.Positional.First() as IShape);
            }

            if (parameters.Positional.Count > 0)
            {
                return ShapeExecuteAsync(parameters.Positional.Cast<IShape>());
            }

            // zero args - no display to execute
            return Task.FromResult<IHtmlContent>(null);
        }

        private async Task<IHtmlContent> ShapeTypeExecuteAsync(string name, INamedEnumerable<object> parameters)
        {
            var shape = await _shapeFactory.CreateAsync(name, parameters);
            return await ShapeExecuteAsync(shape);
        }

        public Task<IHtmlContent> ShapeExecuteAsync(IShape shape)
        {
            // Check if the shape is null or empty.
            if (shape.IsNullOrEmpty())
            {
                return Task.FromResult<IHtmlContent>(HtmlString.Empty);
            }

            // Check if the shape is pre-rendered.
            if (shape is IHtmlContent htmlContent)
            {
                return Task.FromResult(htmlContent);
            }

            var context = new DisplayContext
            {
                DisplayHelper = this,
                Value = shape,
                ServiceProvider = _serviceProvider
            };

            return _htmlDisplay.ExecuteAsync(context);
        }

        public async Task<IHtmlContent> ShapeExecuteAsync(IEnumerable<IShape> shapes)
        {
            var htmlContentBuilder = new HtmlContentBuilder();

            foreach (var shape in shapes)
            {
                htmlContentBuilder.AppendHtml(await ShapeExecuteAsync(shape));
            }

            return htmlContentBuilder;
        }
    }
}
