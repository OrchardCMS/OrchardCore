namespace OrchardCore.DisplayManagement.Html;

public class TheAdminThemeOptions
{
    /// <summary>
    /// Space seperated CSS classes used to control the width of inputs with controls like numeric input.
    /// </summary>
    public string LimitedWidth { get; set; } = "col-md-6 col-lg-4 col-xxl-3";

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
