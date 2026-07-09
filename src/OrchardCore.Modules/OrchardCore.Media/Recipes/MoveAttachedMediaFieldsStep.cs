using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Media.Recipes;

public sealed class MoveAttachedMediaFieldsStep : NamedRecipeStepHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MoveAttachedMediaFieldsStepExecutor _executor;

    public MoveAttachedMediaFieldsStep(
        IHttpContextAccessor httpContextAccessor,
        MoveAttachedMediaFieldsStepExecutor executor)
        : base("move-attached-media-fields")
    {
        _httpContextAccessor = httpContextAccessor;
        _executor = executor;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<MoveAttachedMediaFieldsStepModel>();
        var contentTypes = model?.ContentTypes?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (_httpContextAccessor.HttpContext is null)
        {
            await _executor.ExecuteAsync(contentTypes);

            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("move-attached-media-fields", contentTypes, async (scope, types) =>
        {
            var executor = scope.ServiceProvider.GetRequiredService<MoveAttachedMediaFieldsStepExecutor>();
            await executor.ExecuteAsync(types);
        });
    }
}

public sealed class MoveAttachedMediaFieldsStepModel
{
    public string[] ContentTypes { get; set; }
}
