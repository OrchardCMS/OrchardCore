using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Operations;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Reads the rows of a data set of any registered data source, such as the content items of a content type, the
/// users, or the results of a saved query. It reads with the access of the user the run is for, keeps the selected
/// fields, and keeps the rows that match its filters. Dates in filters are compared in UTC.
/// </summary>
public sealed class DataSourceStep : DataPipelineStepType<DataSourceStepSettings>
{
    /// <summary>
    /// The name of the step type.
    /// </summary>
    public const string StepName = "DataSource";

    private readonly IStringLocalizer S;

    public DataSourceStep(IStringLocalizer<DataSourceStep> localizer)
    {
        S = localizer;
    }

    public override string Name => StepName;

    public override LocalizedString DisplayName => S["Read a data source"];

    public override LocalizedString Description => S["Reads content items, users, query results or any other data set."];

    public override DataPipelineStepCategory Category => DataPipelineStepCategory.Source;

    public override string Icon => "fa-solid fa-database";

    public override async Task DescribeAsync(DataPipelineDescribeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var (_, schema, error) = await GetSchemaAsync(settings, context.Services, context.User, context.CancellationToken);

        if (error is not null)
        {
            context.AddError(error);
            context.SetOutputFields([]);

            return;
        }

        var plan = BuildPlan(schema, settings, out var errors);

        foreach (var message in errors)
        {
            context.AddError(message);
        }

        context.SetOutputFields(plan.Fields);
    }

    public override async Task ExecuteAsync(DataPipelineStepContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var settings = GetSettings(context.Step);
        var (dataSource, schema, error) = await GetSchemaAsync(settings, context.Services, context.Run.User, context.CancellationToken);

        if (error is not null)
        {
            throw new InvalidOperationException(error);
        }

        var plan = BuildPlan(schema, settings, out var errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(errors[0]);
        }

        var today = DateTime.UtcNow.Date;
        var query = new DataSourceQuery
        {
            DataSet = settings.DataSet,
            Context = new DataSourceContext { User = context.Run.User },
            MaxRows = context.Run.IsPreview
                ? (settings.MaxRows > 0 ? Math.Min(settings.MaxRows, context.Run.PreviewRowLimit) : context.Run.PreviewRowLimit)
                : Math.Max(0, settings.MaxRows),
        };

        foreach (var field in plan.ReadFields)
        {
            query.Fields.Add(field);
        }

        foreach (var (name, value) in settings.Parameters ?? [])
        {
            query.Parameters[name] = value;
        }

        var predicates = new List<(string Field, Func<object, bool> Predicate)>();

        foreach (var filter in settings.Filters)
        {
            var field = schema.FindField(filter.Field);
            var predicate = DataFilterPredicates.Build(filter.Operator, field.Type, filter.Values, today);

            if (predicate is null)
            {
                continue;
            }

            predicates.Add((field.Name, predicate));

            if (DataFilterPredicates.BuildCondition(field.Name, filter.Operator, field.Type, filter.Values, value => value, today) is { } condition)
            {
                query.Conditions.Add(condition);
            }
        }

        var output = context.GetOutput();
        var remaining = query.MaxRows > 0 ? query.MaxRows : long.MaxValue;

        await foreach (var batch in dataSource.ReadAsync(query, context.CancellationToken))
        {
            if (output.IsClosed)
            {
                return;
            }

            var indexes = plan.Fields.Select(field => batch.IndexOf(field.Name)).ToArray();
            var filters = predicates.Select(filter => (Index: batch.IndexOf(filter.Field), filter.Predicate)).ToArray();
            var rows = new List<object[]>(batch.Count);

            foreach (var source in batch.Rows)
            {
                if (filters.Any(filter => filter.Index < 0 || !filter.Predicate(source[filter.Index])))
                {
                    continue;
                }

                var row = new object[indexes.Length];

                for (var index = 0; index < indexes.Length; index++)
                {
                    row[index] = indexes[index] >= 0 ? source[indexes[index]] : null;
                }

                rows.Add(row);

                if (--remaining == 0)
                {
                    break;
                }
            }

            if (rows.Count > 0)
            {
                await output.WriteAsync(new DataBatch(plan.Fields, rows), context.CancellationToken);
            }

            if (remaining <= 0)
            {
                return;
            }
        }
    }

    private async Task<(IDataSource DataSource, DataSetSchema Schema, string Error)> GetSchemaAsync(
        DataSourceStepSettings settings,
        IServiceProvider services,
        System.Security.Claims.ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(settings.Source) || string.IsNullOrEmpty(settings.DataSet))
        {
            return (null, null, S["Choose the data source and the data set to read."]);
        }

        var dataSource = services?.GetService<IDataSourceManager>()?.GetDataSource(settings.Source);

        if (dataSource is null)
        {
            return (null, null, S["The data source '{0}' is not available. The feature that provides it may be disabled.", settings.Source]);
        }

        var schema = await dataSource.GetSchemaAsync(settings.DataSet, new DataSourceContext { User = user }, cancellationToken);

        if (schema is null)
        {
            return (dataSource, null, S["The data set '{0}' doesn't exist, or the user the pipeline runs for may not read it.", settings.DataSet]);
        }

        return (dataSource, schema, null);
    }

    private Plan BuildPlan(DataSetSchema schema, DataSourceStepSettings settings, out List<string> errors)
    {
        errors = [];

        var fields = new List<DataField>();

        if (settings.Fields.Count == 0)
        {
            fields.AddRange(schema.Fields);
        }
        else
        {
            foreach (var name in settings.Fields)
            {
                var field = schema.FindField(name);

                if (field is null)
                {
                    errors.Add(S["The data set has no field named '{0}'.", name ?? string.Empty]);

                    continue;
                }

                fields.Add(field);
            }
        }

        var read = new HashSet<string>(fields.Select(field => field.Name), StringComparer.Ordinal);

        foreach (var filter in settings.Filters)
        {
            if (schema.FindField(filter.Field) is null)
            {
                errors.Add(S["The data set has no field named '{0}' to filter on.", filter.Field ?? string.Empty]);

                continue;
            }

            read.Add(filter.Field);
        }

        if (settings.MaxRows < 0)
        {
            errors.Add(S["The number of rows to read can't be negative."]);
        }

        return new Plan(fields.ToArray(), settings.Fields.Count == 0 ? [] : read);
    }

    private sealed record Plan(DataField[] Fields, IEnumerable<string> ReadFields);
}
