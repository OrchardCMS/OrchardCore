using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Core.Body.Model
{
    public class BodyPart : ContentPart
    {
        public string Body { get; set; }
    }
}
