using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
        builder.MapManagementGet(RoutePrefix, ListAsync)
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

        builder.MapManagementGet(RoutePrefix + "/{recipeId}", GetAsync)
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

        builder.MapManagementPost(RoutePrefix + "/{recipeId}:execute", ExecuteAsync)
            .WithName("ApiExecuteRecipe")
            .WithSummary("Executes a recipe.")
            .WithDescription("Executes a non-setup recipe by reusing the existing recipe executor and environment providers.")
            .WithCliCommand(new CliOperationMetadata(["recipes"], "execute")
            {
                Capability = RecipeManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("recipeId", 0) },
                InputMode = CliInputMode.Json,
                RequiresConfirmation = true,
            })
            .Accepts<RecipeExecuteRequest>("application/json")
            .Produces<RecipeExecutionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
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
        IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
        ILogger<AdminController> logger,
        [AsParameters] RecipeListRequest request)
    {
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;

        if (ValidatePaging(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, RecipePermissions.ManageRecipes))
        {
            return httpContext.ApiForbidProblem();
        }

        var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipes = await GetRecipesAsync(features, recipeHarvesters);
        var environment = await GetEnvironmentAsync(environmentProviders, logger);

        var items = recipes
            .Where(recipe => MatchesFilter(recipe, features, request))
            .OrderBy(recipe => recipe.DisplayName ?? recipe.Name, StringComparer.OrdinalIgnoreCase)
            .Select(recipe => ToResponse(recipe, features, environment))
            .ToArray();

        return TypedResults.Ok(new RecipeListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = items.Length,
            Items = items.Skip(skip).Take(take).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(
        HttpContext httpContext,
        string recipeId,
        IAuthorizationService authorizationService,
        IShellFeaturesManager shellFeaturesManager,
        IEnumerable<IRecipeHarvester> recipeHarvesters,
        IEnumerable<IRecipeEnvironmentProvider> environmentProviders,
        ILogger<AdminController> logger,
        IStringLocalizer<AdminController> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, RecipePermissions.ManageRecipes))
        {
            return httpContext.ApiForbidProblem();
        }

        var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipes = await GetRecipesAsync(features, recipeHarvesters);
        var recipe = recipes.FirstOrDefault(candidate => string.Equals(GetRecipeId(candidate), recipeId, StringComparison.Ordinal));
        if (recipe is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Recipe not found."]);
        }

        var environment = await GetEnvironmentAsync(environmentProviders, logger);
        return TypedResults.Ok(ToResponse(recipe, features, environment));
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

        var features = await shellFeaturesManager.GetAvailableFeaturesAsync();
        var recipes = await GetRecipesAsync(features, recipeHarvesters);
        var recipe = recipes.FirstOrDefault(candidate => string.Equals(GetRecipeId(candidate), recipeId, StringComparison.Ordinal));
        if (recipe is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Recipe not found."]);
        }

        var environment = await GetEnvironmentAsync(environmentProviders, logger);
        foreach (var parameter in request.Parameters)
        {
            environment[parameter.Key] = parameter.Value.ValueKind == JsonValueKind.Null
                ? null
                : JsonSerializer.Deserialize<object>(parameter.Value.GetRawText());
        }

        try
        {
            var executionId = Guid.NewGuid().ToString("n");
            await recipeExecutor.ExecuteAsync(executionId, recipe, environment, CancellationToken.None);
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
            return TypedResults.Problem(
                title: localizer["Recipe execution failed."],
                detail: string.Join(' ', exception.StepResult.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception exception) when (!exception.IsFatal())
        {
            logger.LogError(exception, "Unable to execute recipe '{RecipeName}'.", recipe.Name);
            return TypedResults.Problem(
                title: localizer["Recipe execution failed."],
                detail: localizer["Unexpected error occurred while running the '{0}' recipe.", recipe.DisplayName],
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

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

    private static async Task<RecipeDescriptor[]> GetRecipesAsync(IEnumerable<IFeatureInfo> features, IEnumerable<IRecipeHarvester> recipeHarvesters)
    {
        var collections = await Task.WhenAll(recipeHarvesters.Select(harvester => harvester.HarvestRecipesAsync()));
        return collections.SelectMany(collection => collection)
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

    private static Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(title: "Bad request", detail: "Skip must be zero or greater and take must be greater than zero.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (take > 200)
        {
            return TypedResults.Problem(title: "Bad request", detail: "Take cannot exceed 200.", statusCode: StatusCodes.Status400BadRequest);
        }

        return null;
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

    public static RouteHandlerBuilder MapManagementGet(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapGet(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    public static RouteHandlerBuilder MapManagementPost(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapPost(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    private static Action<AuthorizationPolicyBuilder> CreateBearerPolicy()
        => static policy => policy
            .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
            .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement))
            .RequireAuthenticatedUser();
}
