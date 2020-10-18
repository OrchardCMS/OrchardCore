using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentManagement.Display.Models
{
    public class UpdatePartEditorContext : BuildPartEditorContext
    {
        UpdateEditorContext _context;
        public UpdatePartEditorContext(ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
            : base(typePartDefinition, context)
        {
            _context = context;
        }

        public async Task<ValidationResult[]> ValidateAsync<T>(T part) where T : ContentPart
        {
            return await _context.ValidateAsync(TypePartDefinition.Name, part);
        }
    }
}
