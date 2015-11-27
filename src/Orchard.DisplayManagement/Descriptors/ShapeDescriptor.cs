using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Implementation;
using Microsoft.AspNet.Html.Abstractions;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeDescriptor
    {
        public ShapeDescriptor()
        {
            Creating = Enumerable.Empty<Action<ShapeCreatingContext>>();
            Created = Enumerable.Empty<Action<ShapeCreatedContext>>();
            Displaying = Enumerable.Empty<Action<ShapeDisplayingContext>>();
            Displayed = Enumerable.Empty<Action<ShapeDisplayedContext>>();
            Wrappers = new List<string>();
            BindingSources = new List<string>();
            Bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase);
            Placement = ctx => new PlacementInfo { Location = DefaultPlacement };
        }

        public string ShapeType { get; set; }

        /// <summary>
        /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
        /// troubleshooting.
        /// </summary>
        public string BindingSource
        {
            get
            {
                ShapeBinding binding;
                return Bindings.TryGetValue(ShapeType, out binding) ? binding.BindingSource : null;
            }
        }

        public Func<DisplayContext, IHtmlContent> Binding
        {
            get
            {
                return Bindings[ShapeType].Binding;
            }
        }

        public IDictionary<string, ShapeBinding> Bindings { get; set; }

        public IEnumerable<Action<ShapeCreatingContext>> Creating { get; set; }
        public IEnumerable<Action<ShapeCreatedContext>> Created { get; set; }

        public IEnumerable<Action<ShapeDisplayingContext>> Displaying { get; set; }
        public IEnumerable<Action<ShapeDisplayedContext>> Displayed { get; set; }

        public Func<ShapePlacementContext, PlacementInfo> Placement { get; set; }
        public string DefaultPlacement { get; set; }

        public IList<string> Wrappers { get; set; }
        public IList<string> BindingSources { get; set; }
    }

    public class ShapeBinding
    {
        public ShapeDescriptor ShapeDescriptor { get; set; }
        public string BindingName { get; set; }
        public string BindingSource { get; set; }
        public Func<DisplayContext, IHtmlContent> Binding { get; set; }
    }
}