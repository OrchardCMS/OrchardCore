using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.DataSources;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Queries;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Users;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class BuiltInDataSourcesTests
{
    [Fact]
    public async Task ContentItems_GetDataSets_AdminSeesBlogPostWithPartAndFieldColumns()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var source = GetSource(scope, "Contents");
            var dataSourceContext = new DataSourceContext { User = await GetUserAsync(scope, "admin") };

            // Act
            var dataSets = await source.GetDataSetsAsync(dataSourceContext);
            var schema = await source.GetSchemaAsync("BlogPost", dataSourceContext);

            // Assert
            Assert.Contains(dataSets, dataSet => dataSet.Name == "BlogPost" && dataSet.DisplayName == "Blog Post");
            Assert.NotNull(schema);

            var fields = schema.Fields.ToDictionary(field => field.Name);
            Assert.Equal(DataFieldType.Text, fields["ContentItemId"].Type);
            Assert.True(fields["ContentItemId"].IsIdentifier);
            Assert.Equal(DataFieldType.DateTime, fields["CreatedUtc"].Type);
            Assert.Equal(DataFieldType.Boolean, fields["Published"].Type);
            Assert.Equal(DataFieldType.Text, fields["TitlePart.Title"].Type);
            Assert.Equal(DataFieldType.Text, fields["MarkdownBodyPart.Markdown"].Type);
            Assert.Equal(DataFieldType.Text, fields["BlogPost.Subtitle"].Type);
            Assert.Equal(DataFieldType.Text, fields["BlogPost.Tags"].Type);

            // A part value named like its part isn't labeled twice.
            Assert.Equal("Title", fields["TitlePart.Title"].DisplayName);
            Assert.Equal("Autoroute Path", fields["AutoroutePart.Path"].DisplayName);
        });
    }

    [Fact]
    public async Task ContentItems_ReadAsync_ReturnsLatestItemsInBatches()
    {
        // Arrange
        using var context = await CreateContextAsync();

        for (var index = 1; index <= 4; index++)
        {
            var title = $"Post {index}";
            await context.CreateContentItem("BlogPost", builder =>
            {
                builder.DisplayText = title;
                builder.Alter<TitlePartLike>("TitlePart", part => part.Title = title);
            });
        }

        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "Draft post", draft: true);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var source = GetSource(scope, "Contents");
            var query = new DataSourceQuery
            {
                DataSet = "BlogPost",
                BatchSize = 2,
                Context = new DataSourceContext { User = await GetUserAsync(scope, "admin") },
            };

            // Act
            var batches = await DataTestHelpers.ToListAsync(source.ReadAsync(query));

            // Assert
            var rows = batches.SelectMany(batch => batch.Rows).ToList();
            var fields = batches[0].Fields;
            var titles = rows.Select(row => (string)row[fields.IndexOf("DisplayText")]).ToList();

            Assert.All(batches, batch => Assert.True(batch.Count <= 2));
            Assert.Contains("Post 1", titles);
            Assert.Contains("Post 4", titles);
            Assert.Contains("Draft post", titles);
            Assert.Contains(rows, row => (string)row[fields.IndexOf("TitlePart.Title")] == "Post 3");
            Assert.Equal(rows.Count, rows.Select(row => row[fields.IndexOf("ContentItemId")]).Distinct().Count());
        });
    }

    [Fact]
    public async Task ContentItems_ReadAsync_PublishedCondition_ExcludesDrafts()
    {
        // Arrange
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "Draft post", draft: true);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var source = GetSource(scope, "Contents");
            var query = new DataSourceQuery
            {
                DataSet = "BlogPost",
                Context = new DataSourceContext { User = await GetUserAsync(scope, "admin") },
                Conditions = [new DataCondition { Field = "Published", Operator = DataFilterOperator.Equals, Values = [true] }],
            };

            // Act
            var (fields, rows) = await DataTestHelpers.ReadAllAsync(source.ReadAsync(query));

            // Assert
            Assert.NotEmpty(rows);
            Assert.DoesNotContain(rows, row => (string)row[fields.IndexOf("DisplayText")] == "Draft post");
            Assert.All(rows, row => Assert.Equal(true, row[fields.IndexOf("Published")]));
        });
    }

    [Fact]
    public async Task ContentItems_UserWhoCannotEdit_ReadsPublishedItemsOnly()
    {
        // Arrange
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "Draft post", draft: true);

        await context.UsingTenantScopeAsync(async scope =>
        {
            RestrictPermissions(global::OrchardCore.Contents.CommonPermissions.ViewContent);
            var source = GetSource(scope, "Contents");
            var anonymous = new DataSourceContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };

            // Act
            var schema = await source.GetSchemaAsync("BlogPost", anonymous);
            var (fields, rows) = await DataTestHelpers.ReadAllAsync(source.ReadAsync(new DataSourceQuery { DataSet = "BlogPost", Context = anonymous }));

            // Assert
            Assert.NotNull(schema);
            Assert.NotEmpty(rows);
            Assert.DoesNotContain(rows, row => (string)row[fields.IndexOf("DisplayText")] == "Draft post");
        });
    }

    [Fact]
    public async Task Users_AnonymousUser_SeesNothing()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            RestrictPermissions(global::OrchardCore.Contents.CommonPermissions.ViewContent);
            var source = GetSource(scope, "Users");
            var anonymous = new DataSourceContext { User = new ClaimsPrincipal(new ClaimsIdentity()) };

            // Act
            var dataSets = await source.GetDataSetsAsync(anonymous);
            var schema = await source.GetSchemaAsync("Users", anonymous);
            var (_, rows) = await DataTestHelpers.ReadAllAsync(source.ReadAsync(new DataSourceQuery { DataSet = "Users", Context = anonymous }));

            // Assert
            Assert.Empty(dataSets);
            Assert.Null(schema);
            Assert.Empty(rows);
        });
    }

    [Fact]
    public async Task Users_ReadAsync_ReturnsUsersWithoutSecrets()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var source = GetSource(scope, "Users");
            var dataSourceContext = new DataSourceContext { User = await GetUserAsync(scope, "admin") };

            // Act
            var schema = await source.GetSchemaAsync("Users", dataSourceContext);
            var (fields, rows) = await DataTestHelpers.ReadAllAsync(source.ReadAsync(new DataSourceQuery { DataSet = "Users", Context = dataSourceContext }));
            var (roleFields, roleRows) = await DataTestHelpers.ReadAllAsync(source.ReadAsync(new DataSourceQuery { DataSet = "UserRoles", Context = dataSourceContext }));

            // Assert
            Assert.DoesNotContain(schema.Fields, field => field.Name.Contains("Password", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(schema.Fields, field => field.Name.Contains("Stamp", StringComparison.OrdinalIgnoreCase));
            var admin = Assert.Single(rows, row => (string)row[fields.IndexOf("UserName")] == "admin");
            Assert.Equal("Nick@Orchard", admin[fields.IndexOf("Email")]);
            Assert.Equal(true, admin[fields.IndexOf("IsEnabled")]);
            Assert.Contains(roleRows, row => (string)row[roleFields.IndexOf("UserName")] == "admin" && (string)row[roleFields.IndexOf("RoleName")] == "Administrator");
        });
    }

    [Fact]
    public async Task Queries_SavedContentQuery_ExposesContentColumns()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var source = GetSource(scope, "Queries");
            var dataSourceContext = new DataSourceContext { User = await GetUserAsync(scope, "admin") };

            // Act
            var dataSets = await source.GetDataSetsAsync(dataSourceContext);
            var schema = await source.GetSchemaAsync("RecentBlogPosts", dataSourceContext);
            var (fields, rows) = await DataTestHelpers.ReadAllAsync(source.ReadAsync(new DataSourceQuery { DataSet = "RecentBlogPosts", Context = dataSourceContext }));

            // Assert
            Assert.Contains(dataSets, dataSet => dataSet.Name == "RecentBlogPosts");
            Assert.Contains(schema.Fields, field => field.Name == "ContentItemId");
            Assert.NotEmpty(rows);
            Assert.All(rows, row => Assert.Equal("BlogPost", row[fields.IndexOf("ContentType")]));
        });
    }

    [Fact]
    public async Task Queries_SavedSqlQuery_InfersColumnsAndPassesParameters()
    {
        // Arrange
        using var context = await CreateContextAsync();
        await context.CreateContentItem("BlogPost", builder => builder.DisplayText = "Parameter post");

        await context.UsingTenantScopeAsync(async scope =>
        {
            var queryManager = scope.ServiceProvider.GetRequiredService<IQueryManager>();
            var savedQuery = await queryManager.NewAsync("Sql", new JsonObject
            {
                ["Name"] = "ItemsByType",
                ["Template"] = "SELECT DisplayText, DocumentId FROM ContentItemIndex WHERE ContentType = @type AND Latest = 1",
                ["ReturnContentItems"] = false,
            });
            await queryManager.SaveAsync(savedQuery);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var source = GetSource(scope, "Queries");
            var dataSourceContext = new DataSourceContext { User = await GetUserAsync(scope, "admin") };
            var query = new DataSourceQuery { DataSet = "ItemsByType", Context = dataSourceContext };
            query.Parameters["type"] = "BlogPost";

            // Act
            var (fields, rows) = await DataTestHelpers.ReadAllAsync(source.ReadAsync(query));

            // Assert
            Assert.Equal(DataFieldType.Text, fields.Find("DisplayText").Type);
            Assert.Equal(DataFieldType.Integer, fields.Find("DocumentId").Type);
            Assert.Contains(rows, row => (string)row[fields.IndexOf("DisplayText")] == "Parameter post");
        });
    }

    private static async Task<BlogContext> CreateContextAsync()
    {
        var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id == "OrchardCore.DataSources")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });

        return context;
    }

    /// <summary>
    /// Grants only the given permissions for the rest of the current tenant scope. The test site grants every
    /// permission otherwise.
    /// </summary>
    private static void RestrictPermissions(params Permission[] permissions)
    {
        var key = Guid.NewGuid().ToString("n");
        SiteStartup.PermissionsContexts.TryAdd(key, new PermissionsContext { UsePermissionsContext = true, AuthorizedPermissions = permissions });
        SiteContext.HttpContextAccessor.HttpContext.Request.Headers["PermissionsContext"] = key;
    }

    private static IDataSource GetSource(ShellScope scope, string name)
    {
        var source = scope.ServiceProvider.GetRequiredService<IDataSourceManager>().GetDataSource(name);
        Assert.NotNull(source);

        return source;
    }

    private static async Task<ClaimsPrincipal> GetUserAsync(ShellScope scope, string userName)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();
        var factory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<IUser>>();
        var user = await userManager.FindByNameAsync(userName);

        return await factory.CreateAsync(user);
    }

    private sealed class TitlePartLike : ContentPart
    {
        public string Title { get; set; }
    }
}
