using System.ComponentModel;
using System.Text.Json.Serialization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.AuditTrail.Models;

public class AuditTrailPart : ContentPart
{
    [Description("The audit comment recorded for the content item operation.")]
    public string Comment { get; set; }

    [JsonIgnore]
    public bool ShowComment { get; set; }
}
