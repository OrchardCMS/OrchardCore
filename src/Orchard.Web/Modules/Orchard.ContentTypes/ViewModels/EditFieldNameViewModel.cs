using System.ComponentModel.DataAnnotations;

namespace Orchard.ContentTypes.ViewModels
{
    public class EditFieldNameViewModel
    {
        /// <summary>
        /// The technical name of the field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The display name of the field
        /// </summary>
        [Required]
        public string DisplayName { get; set; }
    }
}