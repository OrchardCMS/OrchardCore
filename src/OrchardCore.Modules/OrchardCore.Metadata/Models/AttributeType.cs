using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrchardCore.Metadata.Models
{
    /// <summary>
    /// Enumeration of the possible attribute types for a metadata element.
    /// </summary>
    public enum AttributeType
    {
        [Display(Name = "NAME")]
        name,
        [Display(Name = "PROPERTY")]
        property,
        [Display(Name = "Not applicable")]
        notApplicable
    }
}
