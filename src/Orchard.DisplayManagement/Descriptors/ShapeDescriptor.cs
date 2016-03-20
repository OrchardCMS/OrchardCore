using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Implementation;
using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Descriptors
{
    public class ShapeDescriptor
    {
        public ShapeDescriptor()
        {
            Creating = Enumerable.Empty<Action<ShapeCreatingContext>>();
            Created = Enumerable.Empty<Action<ShapeCreatedContext>>();
            Displaying = Enumerable.Empty<Action<ShapeDisplayingContext>>();
            Processing = Enumerable.Empty<Action<ShapeDisplayingContext>>();
            Displayed = Enumerable.Empty<Action<ShapeDisplayedContext>>();
            Wrappers = new List<string>();
            BindingSources = new List<string>();
            Bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase);
            Placement = DefaultPlacementAction;
        }

        private PlacementInfo DefaultPlacementAction(ShapePlacementContext context)
        {
            // A null default placement means no default placement is specified
            if(DefaultPlacement == null)
            {
                return null;
            }

            return new PlacementInfo { Location = DefaultPlacement };
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

        public Func<DisplayContext, Task<IHtmlContent>> Binding
        {
            get
            {
                return Bindings[ShapeType].BindingAsync;
            }
        }

        public IDictionary<string, ShapeBinding> Bindings { get; set; }

        public IEnumerable<Action<ShapeCreatingContext>> Creating { get; set; }
        public IEnumerable<Action<ShapeCreatedContext>> Created { get; set; }
        public IEnumerable<Action<ShapeDisplayingContext>> Displaying { get; set; }
        public IEnumerable<Action<ShapeDisplayingContext>> Processing { get; set; }
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
        public Func<DisplayContext, Task<IHtmlContent>> BindingAsync { get; set; }
    }
}