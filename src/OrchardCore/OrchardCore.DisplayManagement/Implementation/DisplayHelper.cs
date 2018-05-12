using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DisplayManagement.Implementation
{
    public interface IDisplayHelper
    {
        Task<IHtmlContent> ShapeExecuteAsync(object shape);
    }

    public class DisplayHelper : DynamicObject, IDisplayHelper
    {
        private readonly IHtmlDisplay _htmlDisplay;
        private readonly IShapeFactory _shapeFactory;
        private readonly IServiceProvider _serviceProvider;

        public DisplayHelper(
            IHtmlDisplay htmlDisplay,
            IShapeFactory shapeFactory,
            ViewContext viewContext,
            IServiceProvider serviceProvider)
        {
            _htmlDisplay = htmlDisplay;
            _shapeFactory = shapeFactory;
            ViewContext = viewContext;
            _serviceProvider = serviceProvider;
        }

        public ViewContext ViewContext { get; set; }

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
            if (!string.IsNullOrEmpty(name))
            {
                return ShapeTypeExecuteAsync(name, parameters);
            }

            if (parameters.Positional.Count() == 1)
            {
                return ShapeExecuteAsync(parameters.Positional.First());
            }

            if (parameters.Positional.Any())
            {
                return ShapeExecuteAsync(parameters.Positional);
            }

            // zero args - no display to execute
            return null;
        }

        private async Task<IHtmlContent> ShapeTypeExecuteAsync(string name, INamedEnumerable<object> parameters)
        {
            var shape = await _shapeFactory.CreateAsync(name, parameters);
            return await ShapeExecuteAsync(shape);
        }
        
        public Task<IHtmlContent> ShapeExecuteAsync(object shape)
        {
            if (shape == null)
            {
                return Task.FromResult<IHtmlContent>(HtmlString.Empty);
            }

            var context = new DisplayContext
            {
                DisplayAsync = this,
                Value = shape,
                ViewContext = ViewContext,
                ServiceProvider = _serviceProvider
            };

            return _htmlDisplay.ExecuteAsync(context);
        }

        public async Task<IHtmlContent> ShapeExecuteAsync(IEnumerable<object> shapes)
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