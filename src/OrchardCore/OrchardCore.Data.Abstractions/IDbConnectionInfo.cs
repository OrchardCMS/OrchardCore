namespace OrchardCore.Data;

public interface IDbConnectionInfo
{
    string DatabaseProvider { get; }

    string ConnectionString { get; }

    string TablePrefix { get; }

    string Schema { get; }
}
