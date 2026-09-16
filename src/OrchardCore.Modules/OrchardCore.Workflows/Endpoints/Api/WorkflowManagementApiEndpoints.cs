using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows;

internal static class WorkflowManagementApiEndpoints
{
    internal const string Capability = "workflows";
    private const int DefaultTake = 50;
    private const int MaximumTake = 200;

    public static IEndpointRouteBuilder AddWorkflowManagementApiEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/workflows")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api })
            .RequireAuthorization(policy => policy.AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .DisableAntiforgery();

        group.MapGet("/types", ListWorkflowTypesAsync)
            .WithName("ApiListWorkflowTypes")
            .WithTags("Workflows")
            .WithSummary("Lists workflow types.")
            .WithDescription("Returns the workflow types that the current user can manage.")
            .WithCliCommand(Cli(["workflow", "types"], "list", tableColumns:
            [
                new CliTableColumnMetadata("items[].workflowTypeId", "Id"),
                new CliTableColumnMetadata("items[].name", "Name"),
                new CliTableColumnMetadata("items[].isEnabled", "Enabled"),
            ]))
            .Produces<WorkflowListResponse<WorkflowTypeDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/types/{workflowTypeId}", GetWorkflowTypeAsync)
            .WithName("ApiGetWorkflowType")
            .WithTags("Workflows")
            .WithSummary("Gets a workflow type.")
            .WithDescription("Returns a workflow type, including its activities and transitions.")
            .WithCliCommand(Cli(["workflow", "types"], "show", arguments: [new CliArgumentMetadata("workflowTypeId", 0)]))
            .Produces<WorkflowTypeDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/types", CreateWorkflowTypeAsync)
            .WithName("ApiCreateWorkflowType")
            .WithTags("Workflows")
            .WithSummary("Creates a workflow type.")
            .WithDescription("Creates a workflow type with its activities and transitions.")
            .WithCliCommand(Cli(["workflow", "types"], "create", inputMode: CliInputMode.Json))
            .Produces<WorkflowTypeDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/types/{workflowTypeId}", UpdateWorkflowTypeAsync)
            .WithName("ApiUpdateWorkflowType")
            .WithTags("Workflows")
            .WithSummary("Updates a workflow type.")
            .WithDescription("Replaces a workflow type, including its graph and settings.")
            .WithCliCommand(Cli(["workflow", "types"], "update", arguments: [new CliArgumentMetadata("workflowTypeId", 0)], inputMode: CliInputMode.Json))
            .Produces<WorkflowTypeDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/types/{workflowTypeId}", DeleteWorkflowTypeAsync)
            .WithName("ApiDeleteWorkflowType")
            .WithTags("Workflows")
            .WithSummary("Deletes a workflow type.")
            .WithDescription("Deletes a workflow type and its persisted workflow instances.")
            .WithCliCommand(Cli(["workflow", "types"], "delete", arguments: [new CliArgumentMetadata("workflowTypeId", 0)], requiresConfirmation: true))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/types/validate", ValidateWorkflowTypeAsync)
            .WithName("ApiValidateWorkflowType")
            .WithTags("Workflows")
            .WithSummary("Validates a workflow graph.")
            .WithDescription("Validates a workflow type payload without saving it.")
            .WithCliCommand(Cli(["workflow", "types"], "validate", inputMode: CliInputMode.Json))
            .Produces<WorkflowGraphValidationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/activity-types", ListActivityTypesAsync)
            .WithName("ApiListWorkflowActivityTypes")
            .WithTags("Workflows")
            .WithSummary("Lists workflow activity types.")
            .WithDescription("Returns the registered workflow activities together with their outcome names and basic property schemas.")
            .WithCliCommand(Cli(["workflow", "activity-types"], "list", tableColumns:
            [
                new CliTableColumnMetadata("items[].name", "Name"),
                new CliTableColumnMetadata("items[].category", "Category"),
                new CliTableColumnMetadata("items[].displayText", "Display"),
            ]))
            .Produces<WorkflowListResponse<WorkflowActivityTypeDescriptor>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/activity-types/{name}", GetActivityTypeAsync)
            .WithName("ApiGetWorkflowActivityType")
            .WithTags("Workflows")
            .WithSummary("Gets a workflow activity type.")
            .WithDescription("Returns a registered workflow activity type together with its outcome names and property schema.")
            .WithCliCommand(Cli(["workflow", "activity-types"], "show", arguments: [new CliArgumentMetadata("name", 0)]))
            .Produces<WorkflowActivityTypeDescriptor>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/types/{workflowTypeId}/enable", EnableWorkflowTypeAsync)
            .WithName("ApiEnableWorkflowType")
            .WithTags("Workflows")
            .WithSummary("Enables a workflow type.")
            .WithDescription("Enables a workflow type so it can be executed or triggered.")
            .WithCliCommand(Cli(["workflow", "types"], "enable", arguments: [new CliArgumentMetadata("workflowTypeId", 0)]))
            .Produces<WorkflowTypeDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/types/{workflowTypeId}/disable", DisableWorkflowTypeAsync)
            .WithName("ApiDisableWorkflowType")
            .WithTags("Workflows")
            .WithSummary("Disables a workflow type.")
            .WithDescription("Disables a workflow type so it stops accepting new executions or triggers.")
            .WithCliCommand(Cli(["workflow", "types"], "disable", arguments: [new CliArgumentMetadata("workflowTypeId", 0)]))
            .Produces<WorkflowTypeDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/types/{workflowTypeId}/execute", ExecuteWorkflowAsync)
            .WithName("ApiExecuteWorkflowType")
            .WithTags("Workflows")
            .WithSummary("Executes a workflow type.")
            .WithDescription("Starts a new workflow instance from the specified workflow type.")
            .WithCliCommand(Cli(["workflow", "types"], "execute", arguments: [new CliArgumentMetadata("workflowTypeId", 0)], inputMode: CliInputMode.Json))
            .Produces<WorkflowExecutionResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/instances", ListInstancesAsync)
            .WithName("ApiListWorkflowInstances")
            .WithTags("Workflows")
            .WithSummary("Lists workflow instances.")
            .WithDescription("Lists persisted workflow instances, optionally filtered by workflow type.")
            .WithCliCommand(Cli(["workflow", "instances"], "list"))
            .Produces<WorkflowInstancesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/instances/{workflowId}", GetInstanceAsync)
            .WithName("ApiGetWorkflowInstance")
            .WithTags("Workflows")
            .WithSummary("Gets a workflow instance.")
            .WithDescription("Returns a persisted workflow instance.")
            .WithCliCommand(Cli(["workflow", "instances"], "show", arguments: [new CliArgumentMetadata("workflowId", 0)]))
            .Produces<Workflow>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/instances/{workflowId}", CancelInstanceAsync)
            .WithName("ApiCancelWorkflowInstance")
            .WithTags("Workflows")
            .WithSummary("Cancels a workflow instance.")
            .WithDescription("Cancels a workflow instance by deleting its persisted state.")
            .WithCliCommand(Cli(["workflow", "instances"], "cancel", arguments: [new CliArgumentMetadata("workflowId", 0)], requiresConfirmation: true))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    private static async Task<IResult> ListWorkflowTypesAsync(WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext, int? skip = null, int? take = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        return Page(await service.ListWorkflowTypesAsync(), skip, take);
    }

    private static async Task<IResult> GetWorkflowTypeAsync(string workflowTypeId, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        var workflowType = await service.GetWorkflowTypeAsync(workflowTypeId);
        return workflowType is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(workflowType);
    }

    private static async Task<IResult> CreateWorkflowTypeAsync(WorkflowTypeDto model, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var workflowType = await service.CreateWorkflowTypeAsync(model, modelState);
        return !modelState.IsValid
            ? ValidationProblem(modelState)
            : TypedResults.Created($"/api/workflows/types/{workflowType!.WorkflowTypeId}", workflowType);
    }

    private static async Task<IResult> UpdateWorkflowTypeAsync(string workflowTypeId, WorkflowTypeDto model, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var workflowType = await service.UpdateWorkflowTypeAsync(workflowTypeId, model, modelState);
        if (workflowType is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return !modelState.IsValid ? ValidationProblem(modelState) : TypedResults.Ok(workflowType);
    }

    private static async Task<IResult> DeleteWorkflowTypeAsync(string workflowTypeId, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        await service.DeleteWorkflowTypeAsync(workflowTypeId);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> ValidateWorkflowTypeAsync(WorkflowTypeDto model, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
        => !await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows)
            ? httpContext.ApiForbidProblem()
            : TypedResults.Ok(await service.ValidateWorkflowTypeAsync(model));

    private static async Task<IResult> ListActivityTypesAsync(WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext, int? skip = null, int? take = null)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        return Page(await service.ListActivityTypesAsync(), skip, take);
    }

    private static async Task<IResult> GetActivityTypeAsync(string name, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        var descriptor = await service.GetActivityTypeAsync(name);
        return descriptor is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(descriptor);
    }

    private static Task<IResult> EnableWorkflowTypeAsync(string workflowTypeId, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
        => SetEnabledAsync(workflowTypeId, true, service, authorizationService, httpContext);

    private static Task<IResult> DisableWorkflowTypeAsync(string workflowTypeId, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
        => SetEnabledAsync(workflowTypeId, false, service, authorizationService, httpContext);

    private static async Task<IResult> ExecuteWorkflowAsync(string workflowTypeId, WorkflowExecutionRequest request, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ExecuteWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        var modelState = new ModelStateDictionary();
        var response = await service.ExecuteAsync(workflowTypeId, request, modelState);
        if (response is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return !modelState.IsValid ? ValidationProblem(modelState) : TypedResults.Ok(response);
    }

    private static async Task<IResult> ListInstancesAsync(WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext, string workflowTypeId = null, int skip = 0, int take = 20)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        if (ValidatePaging(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        return TypedResults.Ok(await service.ListInstancesAsync(workflowTypeId, skip, take));
    }

    private static async Task<IResult> GetInstanceAsync(string workflowId, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        var workflow = await service.GetInstanceAsync(workflowId);
        return workflow is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(workflow);
    }

    private static async Task<IResult> CancelInstanceAsync(string workflowId, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        await service.CancelInstanceAsync(workflowId);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> SetEnabledAsync(string workflowTypeId, bool isEnabled, WorkflowApiService service, IAuthorizationService authorizationService, HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, WorkflowsPermissions.ManageWorkflows))
        {
            return httpContext.ApiForbidProblem();
        }

        var workflowType = await service.SetEnabledAsync(workflowTypeId, isEnabled);
        return workflowType is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(workflowType);
    }

    private static CliOperationMetadata Cli(
        string[] commandGroup,
        string verb,
        CliArgumentMetadata[] arguments = null,
        string[] aliases = null,
        CliTableColumnMetadata[] tableColumns = null,
        CliInputMode inputMode = CliInputMode.Options,
        bool requiresConfirmation = false)
    {
        var metadata = new CliOperationMetadata(commandGroup, verb)
        {
            Capability = Capability,
            InputMode = inputMode,
            RequiresConfirmation = requiresConfirmation,
        };

        if (arguments is not null)
        {
            foreach (var argument in arguments)
            {
                metadata.Arguments.Add(argument);
            }
        }

        if (aliases is not null)
        {
            foreach (var alias in aliases)
            {
                metadata.Aliases.Add(alias);
            }
        }

        if (tableColumns is not null)
        {
            foreach (var column in tableColumns)
            {
                metadata.TableColumns.Add(column);
            }
        }

        return metadata;
    }

    private static ValidationProblem ValidationProblem(ModelStateDictionary modelState)
        => TypedResults.ValidationProblem(modelState.ToDictionary(
            entry => entry.Key,
            entry => entry.Value?.Errors.Select(error => error.ErrorMessage).ToArray() ?? []),
            detail: string.Join(", ", modelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))));

    private static IResult Page<T>(IReadOnlyList<T> items, int? requestedSkip, int? requestedTake)
    {
        var skip = requestedSkip ?? 0;
        var take = requestedTake ?? DefaultTake;

        if (ValidatePaging(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        return TypedResults.Ok(new WorkflowListResponse<T>
        {
            Skip = skip,
            Take = take,
            TotalCount = items.Count,
            Items = items.Skip(skip).Take(take).ToArray(),
        });
    }

    private static Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(
                detail: "Skip must be zero or greater and take must be greater than zero.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return take > MaximumTake
            ? TypedResults.Problem(
                detail: $"Take cannot exceed {MaximumTake}.",
                statusCode: StatusCodes.Status400BadRequest)
            : null;
    }
}
