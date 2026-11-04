using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Recipes.Controllers;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Recipes.Endpoints.Management;

internal static class RecipeManagementEndpoints
{
    private const string RoutePrefix = "api/recipes";

    public static IEndpointRouteBuilder AddRecipeManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        var routes = builder.MapManagementGroup(RecipeManagementApiEndpointConventions.TagName);

        routes.MapGet(RoutePrefix, ListAsync)
            .WithName("ApiListRecipes")
            .WithSummary("Lists recipes.")
            .WithDescription("Returns executable non-setup recipes, plus recipe-defined variables and current environment values when they can be derived safely.")
            .WithCliCommand(new CliOperationMetadata(["recipes"], "list")
            {
                Capability = RecipeManagementApiEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items.id", "Id"),
                    new CliTableColumnMetadata("items.displayName", "Name"),
                    new CliTableColumnMetadata("items.feature", "Feature"),
                    new CliTableColumnMetadata("items.fileName", "File"),
                },
            })
            .Produces<RecipeListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        routes.MapGet(RoutePrefix + "/{recipeId}", GetAsync)
            .WithName("ApiGetRecipe")
            .WithSummary("Gets a recipe.")
            .WithDescription("Returns a single executable recipe with parameter information that can be derived safely from the recipe and current environment.")
            .WithCliCommand(new CliOperationMetadata(["recipes"], "show")
            {
                Capability = RecipeManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("recipeId", 0) },
            })
            .Produces<RecipeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapPost(RoutePrefix + "/{recipeId}:validate", ValidateAsync)
            .WithName("ApiValidateRecipe")
            .WithSummary("Validates a recipe without executing it.")
            .WithDescription("Checks that the recipe parses, that each step has a handler, and that the features, themes, files and inner recipes it names exist. It changes nothing and evaluates no script. A valid result doesn't guarantee that execution succeeds, since a step can still fail on its data.")
            .WithCliCommand(new CliOperationMetadata(["recipes"], "validate")
            {
                Capability = RecipeManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("recipeId", 0) },
                ReadOnly = true,
            })
            .Produces<RecipeValidationResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapPost(RoutePrefix + "/{recipeId}:execute", ExecuteAsync)
            .WithName("ApiExecuteRecipe")
            .WithSummary("Executes a recipe.")
            .WithDescription("Executes a non-setup recipe by reusing the existing recipe executor and environment providers. The response waits for the end of the execution. Send a requestId to get the outcome with a status request if the connection is lost, or by repeating the request, which doesn't execute the recipe again.")
            .WithCliCommand(new CliOperationMetadata(["recipes"], "execute")
            {
                Capability = RecipeManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("recipeId", 0) },
                InputMode = CliInputMode.Json,
                DefaultJsonBody = "{}",
                RequiresConfirmation = true,
                LongRunning = true,
            })
            .Accepts<RecipeExecuteRequest>(isOptional: true, "application/json")
            .Produces<RecipeExecutionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        routes.MapGet(RoutePrefix + "/executions/{executionId}", GetExecutionAsync)
            .WithName("ApiGetRecipeExecution")
            .WithSummary("Gets the status of a recipe execution.")
            .WithDescription("Returns the state of an execution, identified by the requestId sent to execute a recipe or by the executionId it returned. The most recent 100 executions are kept.")
            .WithCliCommand(new CliOperationMetadata(["recipes", "executions"], "show")
            {
                Capability = RecipeManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("executionId", 0) },
            })
            .Produces<RecipeExecutionStatusResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    internal static async Task<IResult> ListAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        [FromServices] RecipeDescriptorCache recipeDescriptorCache,
        IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
        ILogger<AdminController> logger,
        [AsParameters] RecipeListRequest request)
    {
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;

        if (httpContext.ApiPagingProblem(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, RecipePermissions.ManageRecipes))
        {
            return httpContext.ApiForbidProblem();
        }

        var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipes = await GetRecipesAsync(features, recipeHarvesters, recipeDescriptorCache);
        var environment = await GetEnvironmentAsync(environmentProviders, logger);

        var filtered = recipes
            .Where(recipe => MatchesFilter(recipe, features, request))
            .OrderBy(recipe => recipe.DisplayName ?? recipe.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return TypedResults.Ok(new RecipeListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = filtered.Length,
            Items = filtered.Skip(skip).Take(take).Select(recipe => ToResponse(recipe, features, environment)).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(
        HttpContext httpContext,
        string recipeId,
        IAuthorizationService authorizationService,
        IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        [FromServices] RecipeDescriptorCache recipeDescriptorCache,
        IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
        ILogger<AdminController> logger,
        IStringLocalizer<AdminController> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, RecipePermissions.ManageRecipes))
        {
            return httpContext.ApiForbidProblem();
        }

        var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipes = await GetRecipesAsync(features, recipeHarvesters, recipeDescriptorCache);
        var recipe = recipes.FirstOrDefault(candidate => string.Equals(GetRecipeId(candidate), recipeId, StringComparison.Ordinal));
        if (recipe is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Recipe not found."]);
        }

        var environment = await GetEnvironmentAsync(environmentProviders, logger);
        return TypedResults.Ok(ToResponse(recipe, features, environment));
    }

    internal static async Task<IResult> ValidateAsync(
        HttpContext httpContext,
        string recipeId,
        IAuthorizationService authorizationService,
        IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        [FromServices] RecipeDescriptorCache recipeDescriptorCache,
        [FromServices] IRecipeValidator recipeValidator,
        IStringLocalizer<AdminController> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, RecipePermissions.ManageRecipes))
        {
            return httpContext.ApiForbidProblem();
        }

        var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipes = await GetRecipesAsync(features, recipeHarvesters, recipeDescriptorCache);
        var recipe = recipes.FirstOrDefault(candidate => string.Equals(GetRecipeId(candidate), recipeId, StringComparison.Ordinal));
        if (recipe is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Recipe not found."]);
        }

        return TypedResults.Ok(await recipeValidator.ValidateAsync(recipe, httpContext.RequestAborted));
    }

    internal static async Task<IResult> ExecuteAsync(
        HttpContext httpContext,
        string recipeId,
        RecipeExecuteRequest request,
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        [FromServices] RecipeDescriptorCache recipeDescriptorCache,
        [FromServices] IRecipeExecutionStore recipeExecutions,
        IRecipeExecutor recipeExecutor,
        IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
        ILogger<AdminController> logger,
        IStringLocalizer<AdminController> localizer)
    {
        request ??= new RecipeExecuteRequest();

        if (!await authorizationService.AuthorizeAsync(httpContext.User, RecipePermissions.ManageRecipes))
        {
            return httpContext.ApiForbidProblem();
        }

        if (request.RequestId is not null && !IsValidRequestId(request.RequestId))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["The request ID must have 1 to 128 letters, digits, hyphens or underscores."]);
        }

        var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipes = await GetRecipesAsync(features, recipeHarvesters, recipeDescriptorCache);
        var recipe = recipes.FirstOrDefault(candidate => string.Equals(GetRecipeId(candidate), recipeId, StringComparison.Ordinal));
        if (recipe is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Recipe not found."]);
        }

        var environment = await GetEnvironmentAsync(environmentProviders, logger);
        // The site provides the identity recipes run with, which callers must not replace.
        var reserved = (request.Parameters ?? []).Keys.FirstOrDefault(environment.ContainsKey);
        if (reserved is not null)
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["The parameter '{0}' is provided by the site and can't be set.", reserved]);
        }

        foreach (var parameter in request.Parameters ?? [])
        {
            environment[parameter.Key] = parameter.Value.ValueKind == JsonValueKind.Null
                ? null
                : JsonSerializer.Deserialize<object>(parameter.Value.GetRawText());
        }

        var executionId = request.RequestId ?? Guid.NewGuid().ToString("n");
        var (started, execution) = await recipeExecutions.TryStartAsync(executionId, recipeId, recipe.Name);
        if (!started)
        {
            return DescribeExistingExecution(execution, recipeId, recipe, localizer);
        }

        try
        {
            // A disconnected client doesn't cancel the execution: its steps commit independently, so stopping
            // would leave the recipe partially applied, while the client can still get the outcome from its status.
            await recipeExecutor.ExecuteAsync(executionId, recipe, environment, CancellationToken.None);
            await recipeExecutions.CompleteAsync(executionId, RecipeExecutionState.Succeeded);
            await shellHost.ReleaseShellContextAsync(shellSettings);

            return TypedResults.Ok(new RecipeExecutionResponse
            {
                ExecutionId = executionId,
                RecipeId = recipeId,
                RecipeName = recipe.Name,
                DisplayName = recipe.DisplayName,
            });
        }
        catch (RecipeExecutionException exception)
        {
            logger.LogError(exception, "Unable to execute recipe '{RecipeName}'.", recipe.Name);
            await recipeExecutions.CompleteAsync(executionId, RecipeExecutionState.Failed, exception.StepResult.Errors);
            return ExecutionFailed(executionId, string.Join(' ', exception.StepResult.Errors), localizer);
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            logger.LogError(exception, "Unable to execute recipe '{RecipeName}'.", recipe.Name);
            string detail = localizer["Unexpected error occurred while running the '{0}' recipe.", recipe.DisplayName];
            await recipeExecutions.CompleteAsync(executionId, RecipeExecutionState.Failed, [detail]);
            return ExecutionFailed(executionId, detail, localizer);
        }
    }

    internal static async Task<IResult> GetExecutionAsync(
        HttpContext httpContext,
        string executionId,
        IAuthorizationService authorizationService,
        [FromServices] IRecipeExecutionStore recipeExecutions,
        IStringLocalizer<AdminController> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, RecipePermissions.ManageRecipes))
        {
            return httpContext.ApiForbidProblem();
        }

        var execution = await recipeExecutions.FindAsync(executionId);
        if (execution is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Recipe execution not found."]);
        }

        return TypedResults.Ok(new RecipeExecutionStatusResponse
        {
            ExecutionId = execution.ExecutionId,
            RecipeId = execution.RecipeId,
            RecipeName = execution.RecipeName,
            State = execution.State.ToString().ToLowerInvariant(),
            Errors = execution.Errors,
            StartedUtc = execution.StartedUtc,
            CompletedUtc = execution.CompletedUtc,
        });
    }

    // Repeating a request reports the outcome of the first one, it never executes the recipe again.
    private static IResult DescribeExistingExecution(RecipeExecution execution, string recipeId, RecipeDescriptor recipe, IStringLocalizer localizer)
    {
        if (execution.RecipeId != recipeId)
        {
            return TypedResults.Problem(detail: localizer["The request ID already belongs to the execution of another recipe."], statusCode: StatusCodes.Status409Conflict);
        }

        return execution.State switch
        {
            RecipeExecutionState.Succeeded => TypedResults.Ok(new RecipeExecutionResponse
            {
                ExecutionId = execution.ExecutionId,
                RecipeId = recipeId,
                RecipeName = recipe.Name,
                DisplayName = recipe.DisplayName,
            }),
            RecipeExecutionState.Failed => ExecutionFailed(execution.ExecutionId, string.Join(' ', execution.Errors), localizer),
            _ => TypedResults.Problem(
                detail: localizer["This request has no outcome yet. Get the status of the execution, and use another request ID to execute the recipe again."],
                statusCode: StatusCodes.Status409Conflict,
                extensions: new Dictionary<string, object> { ["executionId"] = execution.ExecutionId }),
        };
    }

    private static ProblemHttpResult ExecutionFailed(string executionId, string detail, IStringLocalizer localizer)
        => TypedResults.Problem(
            title: localizer["Recipe execution failed."],
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest,
            extensions: new Dictionary<string, object> { ["executionId"] = executionId });

    private static bool IsValidRequestId(string requestId)
        => requestId.Length is > 0 and <= 128 && requestId.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');

    internal static string GetRecipeId(RecipeDescriptor recipe)
        => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes($"{recipe.BasePath}|{recipe.RecipeFileInfo.Name}"));

    internal static async Task<Dictionary<string, object>> GetEnvironmentAsync(IEnumerable<IRecipeEnvironmentProvider> environmentProviders, ILogger logger)
    {
        var environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        await environmentProviders.OrderBy(provider => provider.Order).InvokeAsync((provider, env) => provider.PopulateEnvironmentAsync(env), environment, logger);
        return environment;
    }

    internal static RecipeResponse ToResponse(RecipeDescriptor recipe, IEnumerable<IFeatureInfo> features, IReadOnlyDictionary<string, object> environment)
    {
        var feature = features.FirstOrDefault(candidate => recipe.BasePath != null && candidate.Extension?.SubPath != null && recipe.BasePath.Contains(candidate.Extension.SubPath, StringComparison.OrdinalIgnoreCase));

        return new RecipeResponse
        {
            Id = GetRecipeId(recipe),
            Name = recipe.Name,
            DisplayName = recipe.DisplayName,
            Description = recipe.Description,
            Author = recipe.Author,
            Website = recipe.WebSite,
            Version = recipe.Version,
            Feature = feature?.Name ?? "Application",
            FeatureId = feature?.Id,
            FileName = recipe.RecipeFileInfo.Name,
            BasePath = recipe.BasePath,
            Categories = recipe.Categories ?? [],
            Tags = recipe.Tags ?? [],
            IsSetupRecipe = recipe.IsSetupRecipe,
            Parameters = GetParameters(recipe, environment),
        };
    }

    internal static RecipeParameterResponse[] GetParameters(RecipeDescriptor recipe, IReadOnlyDictionary<string, object> environment)
    {
        var parameters = new Dictionary<string, RecipeParameterResponse>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var stream = recipe.RecipeFileInfo.CreateReadStream();
            using var document = JsonDocument.Parse(stream);

            if (document.RootElement.TryGetProperty("variables", out var variables) && variables.ValueKind == JsonValueKind.Object)
            {
                foreach (var variable in variables.EnumerateObject())
                {
                    parameters[variable.Name] = new RecipeParameterResponse
                    {
                        Name = variable.Name,
                        Source = "recipe",
                        DefaultValue = variable.Value.ToString(),
                    };
                }
            }
        }
        catch
        {
        }

        foreach (var pair in environment)
        {
            if (parameters.TryGetValue(pair.Key, out var existing))
            {
                parameters[pair.Key] = existing with { CurrentValue = pair.Value?.ToString(), Source = existing.Source + "+environment" };
            }
            else
            {
                parameters[pair.Key] = new RecipeParameterResponse
                {
                    Name = pair.Key,
                    Source = "environment",
                    CurrentValue = pair.Value?.ToString(),
                };
            }
        }

        return parameters.Values.OrderBy(parameter => parameter.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static async Task<RecipeDescriptor[]> GetRecipesAsync(IEnumerable<IFeatureInfo> features, IEnumerable<IRecipeHarvester> recipeHarvesters, RecipeDescriptorCache cache)
    {
        var recipes = await cache.GetAsync(recipeHarvesters);
        return recipes
            .Where(recipe => !recipe.IsSetupRecipe
                && (recipe.Tags == null || !recipe.Tags.Contains("hidden", StringComparer.InvariantCultureIgnoreCase))
                && features.Any(feature => recipe.BasePath != null && feature.Extension?.SubPath != null && recipe.BasePath.Contains(feature.Extension.SubPath, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }

    private static bool MatchesFilter(RecipeDescriptor recipe, IEnumerable<IFeatureInfo> features, RecipeListRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search)
            && !recipe.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
            && !(recipe.DisplayName?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false)
            && !(recipe.Description?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Tag) && !(recipe.Tags?.Contains(request.Tag, StringComparer.OrdinalIgnoreCase) ?? false))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.FeatureId)
            && !features.Any(feature => string.Equals(feature.Id, request.FeatureId, StringComparison.OrdinalIgnoreCase)
                && recipe.BasePath != null
                && feature.Extension?.SubPath != null
                && recipe.BasePath.Contains(feature.Extension.SubPath, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }

    internal sealed class RecipeListRequest
    {
        public int? Skip { get; init; }
        public int? Take { get; init; }
        public string Search { get; init; }
        public string Tag { get; init; }
        public string FeatureId { get; init; }
    }

    internal sealed class RecipeExecuteRequest
    {
        /// <summary>Gets an optional caller-generated identifier of the execution, which makes retries safe.</summary>
        public string RequestId { get; init; }

        public Dictionary<string, JsonElement> Parameters { get; init; } = [];
    }

    internal sealed class RecipeListResponse
    {
        public int Skip { get; init; }
        public int Take { get; init; }
        public int TotalCount { get; init; }
        public RecipeResponse[] Items { get; init; } = [];
    }

    internal sealed class RecipeResponse
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; }
        public string DisplayName { get; init; }
        public string Description { get; init; }
        public string Author { get; init; }
        public string Website { get; init; }
        public string Version { get; init; }
        public string Feature { get; init; }
        public string FeatureId { get; init; }
        public string FileName { get; init; }
        public string BasePath { get; init; }
        public string[] Categories { get; init; } = [];
        public string[] Tags { get; init; } = [];
        public bool IsSetupRecipe { get; init; }
        public RecipeParameterResponse[] Parameters { get; init; } = [];
    }

    internal sealed record RecipeParameterResponse
    {
        public string Name { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
        public string DefaultValue { get; init; }
        public string CurrentValue { get; init; }
    }

    internal sealed class RecipeExecutionStatusResponse
    {
        public string ExecutionId { get; init; } = string.Empty;
        public string RecipeId { get; init; } = string.Empty;
        public string RecipeName { get; init; }

        /// <summary>Gets the state: running, succeeded, failed, or uncertain when no outcome was reported in time.</summary>
        public string State { get; init; } = string.Empty;
        public string[] Errors { get; init; } = [];
        public DateTime StartedUtc { get; init; }
        public DateTime? CompletedUtc { get; init; }
    }

    internal sealed class RecipeExecutionResponse
    {
        public string ExecutionId { get; init; } = string.Empty;
        public string RecipeId { get; init; } = string.Empty;
        public string RecipeName { get; init; }
        public string DisplayName { get; init; }
    }
}

internal static class RecipeManagementApiEndpointConventions
{
    public const string CapabilityName = "recipes";
    public const string TagName = "Recipes";
}
