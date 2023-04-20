namespace OrchardCore.DisplayManagement.Html;

public class TheAdminThemeOptions
{
    /// <summary>
    /// Space seperated CSS classes used to control the wrapper.
    /// </summary>
    public string WrapperClasses { get; set; } = "mb-3";

    /// <summary>
    /// Space seperated CSS classes used to control the wrapper for limited width elements.
    /// </summary>
    public string LimitedWidthWrapperClasses { get; set; } = "row";

    //GroupWrapperClasses
    /// <summary>
    /// Space seperated CSS classes used to control the width of input elements like numeric, date, etc.
    /// </summary>
    public string LimitedWidthClasses { get; set; } = "col-md-6 col-lg-4 col-xxl-3";

    /// <summary>
    /// Space seperated CSS classes to add to the leading element like other than a label.
    /// </summary>
    public string StartClasses { get; set; }

    /// <summary>
    /// Space seperated CSS classes to add to the trailing element like input.
    /// </summary>
    public string EndClasses { get; set; }

    /// <summary>
    /// Space seperated CSS classes to add to labels.
    /// </summary>
    public string LabelClasses { get; set; }

    /// <summary>
    /// CSS classes to add ad offset when needed for elements like checkboxes and radio buttons.
    /// </summary>
    public string OffsetClasses { get; set; }
}
