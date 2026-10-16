using System.Data;
using System.Data.Common;

namespace OrchardCore.Data.YesSql.Telemetry;

/// <summary>
/// Wraps the connection of a database provider to trace and measure its commands.
/// </summary>
internal sealed class TelemetryDbConnection : DbConnection, IDbConnectionWrapper
{
    private readonly DbConnection _connection;

    public TelemetryDbConnection(DbConnection connection, DatabaseTelemetryContext context)
    {
        _connection = connection;
        Context = context;
        _connection.StateChange += OnInnerStateChange;
    }

    public DbConnection InnerConnection => _connection;

    public DatabaseTelemetryContext Context { get; }

    public override string ConnectionString
    {
        get => _connection.ConnectionString;
        set => _connection.ConnectionString = value;
    }

    public override int ConnectionTimeout => _connection.ConnectionTimeout;

    public override string Database => _connection.Database;

    public override string DataSource => _connection.DataSource;

    public override string ServerVersion => _connection.ServerVersion;

    public override ConnectionState State => _connection.State;

    public override void ChangeDatabase(string databaseName)
    {
        _connection.ChangeDatabase(databaseName);
    }

    public override void Open() => _connection.Open();

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        return _connection.OpenAsync(cancellationToken);
    }

    public override void Close() => _connection.Close();

    public override Task CloseAsync() => _connection.CloseAsync();

    public override DataTable GetSchema() => _connection.GetSchema();

    public override DataTable GetSchema(string collectionName)
    {
        return _connection.GetSchema(collectionName);
    }

    public override DataTable GetSchema(string collectionName, string[] restrictionValues)
    {
        return _connection.GetSchema(collectionName, restrictionValues);
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return new TelemetryDbTransaction(_connection.BeginTransaction(isolationLevel), this);
    }

    protected override async ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        return new TelemetryDbTransaction(await _connection.BeginTransactionAsync(isolationLevel, cancellationToken), this);
    }

    protected override DbCommand CreateDbCommand()
    {
        return new TelemetryDbCommand(_connection.CreateCommand(), this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.StateChange -= OnInnerStateChange;
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        _connection.StateChange -= OnInnerStateChange;
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }

    private void OnInnerStateChange(object sender, StateChangeEventArgs e) => OnStateChange(e);
}
