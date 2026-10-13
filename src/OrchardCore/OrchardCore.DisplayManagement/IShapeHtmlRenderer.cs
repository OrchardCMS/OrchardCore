namespace OrchardCore.DisplayManagement;

/// <summary>
/// Renders shapes to HTML outside of a view, such as in an endpoint that returns the markup of a shape in a JSON
/// response. Shape templates render as partial views of the current view; without one, the first template of the
/// request would render as a main page, wrapped in the theme layout, so they are rendered in a view context without
/// output instead.
/// </summary>
public interface IShapeHtmlRenderer
{
    /// <summary>
    /// Renders a shape.
    /// </summary>
    /// <param name="shape">The shape to render.</param>
    /// <param name="cancellationToken">A token to stop the rendering before it starts, such as when the request is aborted.</param>
    /// <returns>The HTML of the shape.</returns>
    Task<string> RenderAsync(IShape shape, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a zone of a shape, such as the <c>Content</c> zone where the drivers place their shapes, without the
    /// template of the shape itself.
    /// </summary>
    /// <param name="shape">The shape that holds the zone.</param>
    /// <param name="zoneName">The name of the zone.</param>
    /// <param name="cancellationToken">A token to stop the rendering before it starts, such as when the request is aborted.</param>
    /// <returns>The HTML of the zone, or an empty string when the shape has no such zone or it is empty.</returns>
    Task<string> RenderZoneAsync(IShape shape, string zoneName, CancellationToken cancellationToken = default);
}
