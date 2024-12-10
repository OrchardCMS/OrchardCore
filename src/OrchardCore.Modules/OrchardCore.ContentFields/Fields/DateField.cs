using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class DateField : ContentField
{
    public DateOnly? Value { get; set; }
}
