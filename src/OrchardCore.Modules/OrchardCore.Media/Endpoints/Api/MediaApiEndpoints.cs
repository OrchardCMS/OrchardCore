namespace OrchardCore.Media.Endpoints.Api;

/// <summary>
/// Marker type used as the resource/category anchor for the Media API endpoints' localization
/// (<see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/>) and logging
/// (<see cref="Microsoft.Extensions.Logging.ILogger{T}"/>). It replaces the former
/// <c>MediaApiController</c>, whose actions are now minimal-API endpoints.
/// </summary>
public sealed class MediaApiEndpoints;
