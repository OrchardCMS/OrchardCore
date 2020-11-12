using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.DisplayManagement.Handlers
{
    public class UpdateEditorContext : BuildEditorContext
    {
        private ValidationHandler _validationHandler;

        public UpdateEditorContext(IShape model, string groupId, bool isNew, string htmlFieldPrefix, IShapeFactory shapeFactory,
            IShape layout, IUpdateModel updater)
            : base(model, groupId, isNew, htmlFieldPrefix, shapeFactory, layout, updater)
        {
        }

        public delegate Task<ValidationResult[]> ValidationHandler(string partName, object model);

        public Task<ValidationResult[]> ValidateAsync<T>(string partName, T model)
        {
            if (_validationHandler == null)
            {
                throw new InvalidOperationException($"{nameof(ValidateAsync)} function is not set.");
            }
            return _validationHandler.Invoke(partName, model);
        }

        public void SetValidationHandler(ValidationHandler handler)
        {
            _validationHandler = handler;
        }
    }
}
