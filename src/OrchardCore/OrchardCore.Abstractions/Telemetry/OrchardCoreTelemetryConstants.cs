namespace OrchardCore.Telemetry;

/// <summary>
/// The names of the sources, instruments and attributes of the telemetry emitted by Orchard Core.
/// </summary>
public static class OrchardCoreTelemetryConstants
{
    /// <summary>
    /// The name of the <see cref="System.Diagnostics.ActivitySource"/> of the traces emitted by Orchard Core.
    /// </summary>
    public const string ActivitySourceName = "OrchardCore";

    /// <summary>
    /// The name of the <see cref="System.Diagnostics.Metrics.Meter"/> of the metrics emitted by Orchard Core.
    /// </summary>
    public const string MeterName = "OrchardCore";

    /// <summary>
    /// The names of the attributes added to the traces, metrics and logs.
    /// </summary>
    public static class Attributes
    {
        /// <summary>
        /// The name of the tenant.
        /// </summary>
        public const string TenantName = "orchardcore.tenant.name";

        /// <summary>
        /// The state of the tenant, e.g. 'Running' or 'Disabled'.
        /// </summary>
        public const string TenantState = "orchardcore.tenant.state";

        /// <summary>
        /// Why the tenant was released: 'reload', 'release' or 'remove'.
        /// </summary>
        public const string TenantReleaseReason = "orchardcore.tenant.release.reason";

        /// <summary>
        /// The name of the background task.
        /// </summary>
        public const string BackgroundTaskName = "orchardcore.background_task.name";

        /// <summary>
        /// The name of the recipe.
        /// </summary>
        public const string RecipeName = "orchardcore.recipe.name";

        /// <summary>
        /// The name of the recipe step.
        /// </summary>
        public const string RecipeStepName = "orchardcore.recipe.step.name";

        /// <summary>
        /// The type of the error that ended the operation, as defined by the OpenTelemetry semantic conventions.
        /// </summary>
        public const string ErrorType = "error.type";

        /// <summary>
        /// The database management system, e.g. 'sqlite' or 'postgresql', as defined by the OpenTelemetry semantic conventions.
        /// </summary>
        public const string DbSystemName = "db.system.name";

        /// <summary>
        /// The name of the database, as defined by the OpenTelemetry semantic conventions.
        /// </summary>
        public const string DbNamespace = "db.namespace";

        /// <summary>
        /// The database operation, e.g. 'SELECT', as defined by the OpenTelemetry semantic conventions.
        /// </summary>
        public const string DbOperationName = "db.operation.name";

        /// <summary>
        /// The table the database operation targets, as defined by the OpenTelemetry semantic conventions.
        /// </summary>
        public const string DbCollectionName = "db.collection.name";

        /// <summary>
        /// The SQL text of the database operation, as defined by the OpenTelemetry semantic conventions.
        /// </summary>
        public const string DbQueryText = "db.query.text";

        /// <summary>
        /// The number of database operations run by an HTTP request.
        /// </summary>
        public const string DbOperations = "orchardcore.db.operations";

        /// <summary>
        /// The total duration, in seconds, of the database operations run by an HTTP request.
        /// </summary>
        public const string DbDuration = "orchardcore.db.duration";

        /// <summary>
        /// The route of an HTTP request, as defined by the OpenTelemetry semantic conventions.
        /// </summary>
        public const string HttpRoute = "http.route";

        /// <summary>
        /// The type of a shape, e.g. 'Content' or 'MenuItem'.
        /// </summary>
        public const string ShapeType = "orchardcore.shape.type";

        /// <summary>
        /// The name of the binding that rendered a shape, i.e. the alternate that was used, e.g. 'Content__BlogPost'.
        /// </summary>
        public const string ShapeBinding = "orchardcore.shape.binding";

        /// <summary>
        /// The source of the binding that rendered a shape, e.g. the path of its template.
        /// </summary>
        public const string ShapeBindingSource = "orchardcore.shape.binding.source";

        /// <summary>
        /// Whether the content of a shape was found in its cache: 'hit' or 'miss'.
        /// </summary>
        public const string ShapeCacheResult = "orchardcore.shape.cache.result";
    }

    /// <summary>
    /// The names of the metric instruments.
    /// </summary>
    public static class Instruments
    {
        /// <summary>
        /// The duration, in seconds, of building the service container of a tenant.
        /// </summary>
        public const string TenantContainerDuration = "orchardcore.tenant.container.duration";

        /// <summary>
        /// The duration, in seconds, of building the request pipeline of a tenant.
        /// </summary>
        public const string TenantPipelineDuration = "orchardcore.tenant.pipeline.duration";

        /// <summary>
        /// The duration, in seconds, of activating a tenant.
        /// </summary>
        public const string TenantActivationDuration = "orchardcore.tenant.activation.duration";

        /// <summary>
        /// The number of times a tenant was released, so that it gets rebuilt on its next use.
        /// </summary>
        public const string TenantReleases = "orchardcore.tenant.releases";

        /// <summary>
        /// The duration, in seconds, of running a background task.
        /// </summary>
        public const string BackgroundTaskDuration = "orchardcore.background_task.duration";

        /// <summary>
        /// The duration, in seconds, of a database operation, as defined by the OpenTelemetry semantic conventions.
        /// </summary>
        public const string DbClientOperationDuration = "db.client.operation.duration";

        /// <summary>
        /// The number of database operations run by an HTTP request.
        /// </summary>
        public const string RequestDbOperations = "orchardcore.request.db.operations";

        /// <summary>
        /// The total duration, in seconds, of the database operations run by an HTTP request.
        /// </summary>
        public const string RequestDbDuration = "orchardcore.request.db.duration";

        /// <summary>
        /// The duration, in seconds, of rendering a shape, including the shapes it renders.
        /// </summary>
        public const string ShapeRenderDuration = "orchardcore.shape.render.duration";

        /// <summary>
        /// The number of lookups of the content of a cached shape, by result.
        /// </summary>
        public const string ShapeCacheLookups = "orchardcore.shape.cache.lookups";

        /// <summary>
        /// The number of tenants by state.
        /// </summary>
        public const string Tenants = "orchardcore.tenants";

        /// <summary>
        /// The number of tenants whose service container is built and loaded in memory.
        /// </summary>
        public const string LoadedTenants = "orchardcore.tenants.loaded";
    }
}
