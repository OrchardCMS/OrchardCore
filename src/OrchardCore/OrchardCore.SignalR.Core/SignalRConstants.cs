namespace OrchardCore.SignalR;

/// <summary>
/// Contains constant values for the SignalR module.
/// </summary>
public static class SignalRConstants
{
    /// <summary>
    /// Contains the identifiers of the features provided by the SignalR module.
    /// </summary>
    public static class Feature
    {
        /// <summary>
        /// The identifier of the base SignalR feature, which registers SignalR, the client
        /// resources, and hub authentication.
        /// </summary>
        public const string Area = "OrchardCore.SignalR";

        /// <summary>
        /// The identifier of the feature that enables a tenant-qualified Redis backplane for SignalR.
        /// </summary>
        public const string RedisBackplane = "OrchardCore.SignalR.Redis";

        /// <summary>
        /// The identifier of the feature that enables the Azure SignalR Service backplane.
        /// </summary>
        public const string AzureBackplane = "OrchardCore.SignalR.Azure";
    }
}
