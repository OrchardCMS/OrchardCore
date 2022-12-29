namespace OrchardCore.Data;

public interface IShellDatabaseInfo
{
    string DatabaseProvider { get; }

    string ConnectionString { get; }

    string TablePrefix { get; }

    string Schema { get; }

    string Name { get; }
}
