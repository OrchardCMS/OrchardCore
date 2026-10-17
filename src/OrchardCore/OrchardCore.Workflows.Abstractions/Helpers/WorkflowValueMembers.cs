using Microsoft.Extensions.Localization;
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
    public static IReadOnlyList<ActivityProvidedValueMember> ContentItem(IStringLocalizer S)
        =>
        [
            Member("ContentItemId", "string", S["The id of the content item."]),
            Member("ContentItemVersionId", "string", S["The id of this version of the content item."]),
            Member("ContentType", "string", S["The content type."]),
            Member("DisplayText", "string", S["The display text, its title."]),
            Member("Owner", "string", S["The id of the user who owns it."]),
            Member("Author", "string", S["The name of the user who last changed it."]),
            Member("Published", "boolean", S["Whether this version is published."]),
            Member("Latest", "boolean", S["Whether this is the latest version."]),
            Member("CreatedUtc", "datetime", S["When it was created."]),
            Member("ModifiedUtc", "datetime", S["When it was last changed."]),
            Member("PublishedUtc", "datetime", S["When it was published."]),
        ];

    /// <summary>
    /// The fields of a content event (<c>ContentEventContext</c>).
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> ContentEvent(IStringLocalizer S)
        =>
        [
            Member("Name", "string", S["The name of the event, for example Published."]),
            Member("ContentType", "string", S["The content type of the content item."]),
            Member("ContentItemId", "string", S["The id of the content item."]),
            Member("ContentItemVersionId", "string", S["The id of the version of the content item."]),
            Member("IsStart", "boolean", S["Whether the event started the workflow."]),
        ];

    /// <summary>
    /// The fields of a user.
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> User(IStringLocalizer S)
        =>
        [
            Member("UserId", "string", S["The id of the user."]),
            Member("UserName", "string", S["The user name."]),
            Member("Email", "string", S["The email address."]),
            Member("EmailConfirmed", "boolean", S["Whether the email address is confirmed."]),
            Member("IsEnabled", "boolean", S["Whether the user is enabled."]),
            Member("RoleNames", "array", S["The names of the user's roles."]),
        ];

    /// <summary>
    /// The fields of a workflow fault (<c>WorkflowFaultModel</c>).
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> WorkflowFault(IStringLocalizer S)
        =>
        [
            Member("WorkflowName", "string", S["The name of the workflow that faulted."]),
            Member("WorkflowId", "string", S["The id of the instance that faulted."]),
            Member("ActivityId", "string", S["The id of the activity that faulted."]),
            Member("ActivityDisplayName", "string", S["The name of the activity that faulted."]),
            Member("ActivityTypeName", "string", S["The type of the activity that faulted."]),
            Member("ErrorMessage", "string", S["The message of the error."]),
            Member("FaultMessage", "string", S["The fault message of the instance."]),
            Member("ExceptionDetails", "string", S["The details of the error."]),
            Member("ExecutedActivityCount", "number", S["How many activities the instance ran."]),
        ];

    /// <summary>
    /// The fields of a <c>Result</c>, which services such as the email and SMS services return.
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> Result(IStringLocalizer S)
        =>
        [
            Member("Succeeded", "boolean", S["Whether it succeeded."]),
            Member("Errors", "array", S["The errors, when it failed."]),
        ];

    /// <summary>
    /// The fields of the response to an HTTP request.
    /// </summary>
    public static IReadOnlyList<ActivityProvidedValueMember> HttpResponse(IStringLocalizer S)
        =>
        [
            Member("Body", "string", S["The body of the response."]),
            Member("Headers", "object", S["The headers of the response, by name."]),
            Member("StatusCode", "number", S["The status code, for example 200."]),
            Member("ReasonPhrase", "string", S["The reason phrase, for example OK."]),
            Member("IsSuccessStatusCode", "boolean", S["Whether the status code is 2xx."]),
        ];

    private static ActivityProvidedValueMember Member(string name, string typeName, LocalizedString description)
        => new() { Name = name, TypeName = typeName, Description = description };
}
