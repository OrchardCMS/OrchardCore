using System.Data.Common;
using YesSql;

namespace OrchardCore.Data.YesSql.Telemetry;

/// <summary>
/// Creates connections whose commands are traced and measured, while anything listens to the database telemetry.
/// </summary>
internal sealed class TelemetryConnectionFactory : IConnectionFactory
{
    private readonly IConnectionFactory _factory;
    private readonly DatabaseTelemetryContext _context;

    public TelemetryConnectionFactory(IConnectionFactory factory, DatabaseTelemetryContext context)
    {
        _factory = factory;
        _context = context;
    }

    public Type DbConnectionType => _factory.DbConnectionType;

    public DbConnection CreateConnection()
    {
        var connection = _factory.CreateConnection();

        // Nothing is wrapped while nothing listens, so the telemetry costs nothing then.
        return DatabaseTelemetry.IsEnabled
            ? new TelemetryDbConnection(connection, _context)
            : connection;
    }
}
