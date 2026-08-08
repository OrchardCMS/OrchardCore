namespace OrchardCore.SignalR;

/// <summary>
/// Indicates that a SignalR hub accepts the OrchardCore <c>Api</c> authentication scheme, allowing token based
/// clients such as headless front-ends, mobile applications, and service-to-service callers to connect to the hub
/// the same way they call the API endpoints.
/// </summary>
/// <remarks>
/// Hubs are opt-in. Without this attribute the hub keeps the default behavior, where only the authentication
/// schemes configured by the host, such as the cookie scheme, are evaluated. Applying the attribute never weakens
/// the hub's authorization requirements. It only allows an otherwise anonymous request that carries a valid bearer
/// token to be associated with the token's user before authorization runs.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AllowApiTokenAuthenticationAttribute : Attribute
{
}
