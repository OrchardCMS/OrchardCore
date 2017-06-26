using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.DisplayManagement.Fluid.ModelBinding
{
    public class ModelStateNode : ModelStateEntry
    {
        private readonly ModelStateEntry _entry;

        public ModelStateNode(ModelStateEntry entry)
        {
            _entry = entry;
            RawValue = entry.RawValue;
            AttemptedValue = entry.AttemptedValue;

            Errors.Clear();
            for (var i = 0; i < entry.Errors.Count; i++)
            {
                Errors.Add(entry.Errors[i]);
            }

            ValidationState = entry.ValidationState;
        }

        public override bool IsContainerNode => _entry.IsContainerNode;

        public override IReadOnlyList<ModelStateEntry> Children =>
            _entry.Children.Select(entry => new ModelStateNode(entry)).ToList();

        public override ModelStateEntry GetModelStateForProperty(string propertyName)
        {
            return new ModelStateNode(_entry.GetModelStateForProperty(propertyName));
        }
    }
}
