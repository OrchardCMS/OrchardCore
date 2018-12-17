# Data (OrchardCore.Data)

## Access IDbConnection via DI (Dependancy Injection)

Sometimes it is necessary to get access to the database so you can write your own data to the database.   You may want to create your own tables and do CRUD operations on them.   If you use an ORM like dapper you need access to an instance IDbConnection.   We now provide the IDbConnectionAccessor via DI (Dependency Injection) for you to do custom things.   Just make sure you either close the connection or Dispose of the DbConnecionAccessor when you are done with it.

```C#
public class AdminController : Controller
{
    private readonly IDbConnectionAccessor _dbAccessor;

    public AdminController(IDbConnectionAccessor dbAccessor)
    {
        _dbAccessor = dbAccessor;
    }

    public async Task<ActionResult> Index()
    {
       //TODO: Make sure you put the call to `DbConnectionAccessor.GetConnectionAsyc()` in a using block or explicitly close the connection when you are done with it.

       using (var connection = await _dbAccessor.GetConnectionAsyc())
       {
           //TODO:  You can use Dapper or any other ORM to to custom data access here.
       }
       return View();
    }    
}
```

```C#
public class UpdateManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ShellSettings _shellSettings;      
    private readonly ISession _session;
    private readonly ILogger _logger;
    private ShellDescriptor _shellDescriptor;

    public UpdateManager(
        IServiceProvider serviceProvider,
        ShellSettings shellSettings,          
        ISession session,
        ILogger<ShellDescriptorManager> logger)
    {
        _serviceProvider = serviceProvider;
        _shellSettings = shellSettings;          
        _session = session;
        _logger = logger;
    }       

    private async Task UpdateSomeTable()
    {
        // TODO: Make sure that you use the DbConnectionAccessor in a using block or explicly call DbConnectionAccessor.Dispose() when you are done using it.    

        using (var connectionAccessor = _serviceProvider.GetRequiredService<IDbConnectionAccessor>() as DbConnectionAccessor)
        {
            var connection = await connectionAccessor.GetConnectionAsync();
            var dialect = SqlDialectFactory.For(connection);
            var tablePrefix = _shellSettings.TablePrefix;
            var customTable = dialect.QuoteForTableName($"{tablePrefix}CustomTable");             

            var updateCommand = $"UPDATE {customTable} SET Name = 'Foo Bar' WHERE Id = 1";
               
            await connection.ExecuteAsync(updateCommand);               
        }
    }
}


