using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.Helpers;

/// <summary>
/// The fields of values that several activities provide, for their <see cref="ActivityProvidedValue.Members"/>.
/// </summary>
public static class WorkflowValueMembers
{
    /// <summary>
    /// The fields of a content item.
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> ContentItem { get; } =
        [
            Member("ContentItemId", "string", LocalizationSource.Create("The id of the content item.", typeof(WorkflowValueMembers))),
            Member("ContentItemVersionId", "string", LocalizationSource.Create("The id of this version of the content item.", typeof(WorkflowValueMembers))),
            Member("ContentType", "string", LocalizationSource.Create("The content type.", typeof(WorkflowValueMembers))),
            Member("DisplayText", "string", LocalizationSource.Create("The display text, its title.", typeof(WorkflowValueMembers))),
            Member("Owner", "string", LocalizationSource.Create("The id of the user who owns it.", typeof(WorkflowValueMembers))),
            Member("Author", "string", LocalizationSource.Create("The name of the user who last changed it.", typeof(WorkflowValueMembers))),
            Member("Published", "boolean", LocalizationSource.Create("Whether this version is published.", typeof(WorkflowValueMembers))),
            Member("Latest", "boolean", LocalizationSource.Create("Whether this is the latest version.", typeof(WorkflowValueMembers))),
            Member("CreatedUtc", "datetime", LocalizationSource.Create("When it was created.", typeof(WorkflowValueMembers))),
            Member("ModifiedUtc", "datetime", LocalizationSource.Create("When it was last changed.", typeof(WorkflowValueMembers))),
            Member("PublishedUtc", "datetime", LocalizationSource.Create("When it was published.", typeof(WorkflowValueMembers))),
        ];

    /// <summary>
    /// The fields of a content event (<c>ContentEventContext</c>).
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> ContentEvent { get; } =
        [
            Member("Name", "string", LocalizationSource.Create("The name of the event, for example Published.", typeof(WorkflowValueMembers))),
            Member("ContentType", "string", LocalizationSource.Create("The content type of the content item.", typeof(WorkflowValueMembers))),
            Member("ContentItemId", "string", LocalizationSource.Create("The id of the content item.", typeof(WorkflowValueMembers))),
            Member("ContentItemVersionId", "string", LocalizationSource.Create("The id of the version of the content item.", typeof(WorkflowValueMembers))),
            Member("IsStart", "boolean", LocalizationSource.Create("Whether the event started the workflow.", typeof(WorkflowValueMembers))),
        ];

    /// <summary>
    /// The fields of a user.
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> User { get; } =
        [
            Member("UserId", "string", LocalizationSource.Create("The id of the user.", typeof(WorkflowValueMembers))),
            Member("UserName", "string", LocalizationSource.Create("The user name.", typeof(WorkflowValueMembers))),
            Member("Email", "string", LocalizationSource.Create("The email address.", typeof(WorkflowValueMembers))),
            Member("EmailConfirmed", "boolean", LocalizationSource.Create("Whether the email address is confirmed.", typeof(WorkflowValueMembers))),
            Member("IsEnabled", "boolean", LocalizationSource.Create("Whether the user is enabled.", typeof(WorkflowValueMembers))),
            Member("RoleNames", "array", LocalizationSource.Create("The names of the user's roles.", typeof(WorkflowValueMembers))),
        ];

    /// <summary>
    /// The fields of a workflow fault (<c>WorkflowFaultModel</c>).
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> WorkflowFault { get; } =
        [
            Member("WorkflowName", "string", LocalizationSource.Create("The name of the workflow that faulted.", typeof(WorkflowValueMembers))),
            Member("WorkflowId", "string", LocalizationSource.Create("The id of the instance that faulted.", typeof(WorkflowValueMembers))),
            Member("ActivityId", "string", LocalizationSource.Create("The id of the activity that faulted.", typeof(WorkflowValueMembers))),
            Member("ActivityDisplayName", "string", LocalizationSource.Create("The name of the activity that faulted.", typeof(WorkflowValueMembers))),
            Member("ActivityTypeName", "string", LocalizationSource.Create("The type of the activity that faulted.", typeof(WorkflowValueMembers))),
            Member("ErrorMessage", "string", LocalizationSource.Create("The message of the error.", typeof(WorkflowValueMembers))),
            Member("FaultMessage", "string", LocalizationSource.Create("The fault message of the instance.", typeof(WorkflowValueMembers))),
            Member("ExceptionDetails", "string", LocalizationSource.Create("The details of the error.", typeof(WorkflowValueMembers))),
            Member("ExecutedActivityCount", "number", LocalizationSource.Create("How many activities the instance ran.", typeof(WorkflowValueMembers))),
        ];

    /// <summary>
    /// The fields of a <c>Result</c>, which services such as the email and SMS services return.
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> Result { get; } =
        [
            Member("Succeeded", "boolean", LocalizationSource.Create("Whether it succeeded.", typeof(WorkflowValueMembers))),
            Member("Errors", "array", LocalizationSource.Create("The errors, when it failed.", typeof(WorkflowValueMembers))),
        ];

    /// <summary>
    /// The fields of the response to an HTTP request.
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> HttpResponse { get; } =
        [
            Member("Body", "string", LocalizationSource.Create("The body of the response.", typeof(WorkflowValueMembers))),
            Member("Headers", "object", LocalizationSource.Create("The headers of the response, by name.", typeof(WorkflowValueMembers))),
            Member("StatusCode", "number", LocalizationSource.Create("The status code, for example 200.", typeof(WorkflowValueMembers))),
            Member("ReasonPhrase", "string", LocalizationSource.Create("The reason phrase, for example OK.", typeof(WorkflowValueMembers))),
            Member("IsSuccessStatusCode", "boolean", LocalizationSource.Create("Whether the status code is 2xx.", typeof(WorkflowValueMembers))),
        ];


    private static ActivityProvidedValueMember Member(string name, string typeName, LocalizationSource description)
        => new() { Name = name, TypeName = typeName, Description = description };
}
