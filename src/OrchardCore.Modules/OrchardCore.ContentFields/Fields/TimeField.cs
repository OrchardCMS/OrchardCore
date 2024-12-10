using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class TimeField : ContentField
{
    public TimeOnly? Value { get; set; }
}
