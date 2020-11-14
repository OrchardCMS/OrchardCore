using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.ContentManagement.Display.Models
{
    public class UpdatePartEditorContext : BuildPartEditorContext
    {
        private readonly UpdateEditorContext _context;

        public UpdatePartEditorContext(ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
            : base(typePartDefinition, context)
        {
            _context = context;
        }

        public Task<ValidationResult[]> ValidateAsync<T>(T part) where T : ContentPart
        {
            return _context.ValidateAsync(TypePartDefinition.Name, part);
        }
    }
}
