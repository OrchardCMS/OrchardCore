using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using YesSql;
using YesSql.Filters.Nodes;
using YesSql.Filters.Query;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class DefaultContentsAdminListFilterProviderTests
{
    private const string UserId = "user-id";

    [Fact]
    public async Task TypeFilterShouldSupportMultipleValuesAndOwnership()
    {
        var definitions = new[]
        {
            CreateContentType("Article"),
            CreateContentType("BlogPost"),
        };

        var predicates = await ExecuteFilterAsync(
            new ContentTypeFilterNode(["Article", "Missing", "BlogPost"]),
            definitions,
            contentItem => contentItem.ContentType == "Article");

        var article = new ContentItemIndex { ContentType = "Article", Owner = "other-user" };
        var ownedBlogPost = new ContentItemIndex { ContentType = "BlogPost", Owner = UserId };
        var otherBlogPost = new ContentItemIndex { ContentType = "BlogPost", Owner = "other-user" };
        var page = new ContentItemIndex { ContentType = "Page", Owner = UserId };

        Assert.Contains(predicates, predicate =>
            predicate(article) &&
            predicate(ownedBlogPost) &&
            !predicate(otherBlogPost) &&
            !predicate(page));
    }

    [Fact]
    public async Task StereotypeFilterShouldMatchAnySpecifiedStereotype()
    {
        var definitions = new[]
        {
            CreateContentType("Article", "Page"),
            CreateContentType("HtmlWidget", "Widget"),
            CreateContentType("Menu", "Menu"),
        };

        var predicates = await ExecuteFilterAsync(
            new StereotypeFilterNode(["Page", "Widget"]),
            definitions,
            contentItem => contentItem.ContentType == "Article");

        var article = new ContentItemIndex { ContentType = "Article", Owner = "other-user" };
        var ownedHtmlWidget = new ContentItemIndex { ContentType = "HtmlWidget", Owner = UserId };
        var otherHtmlWidget = new ContentItemIndex { ContentType = "HtmlWidget", Owner = "other-user" };
        var menu = new ContentItemIndex { ContentType = "Menu", Owner = UserId };

        Assert.Contains(predicates, predicate =>
            predicate(article) &&
            predicate(ownedHtmlWidget) &&
            !predicate(otherHtmlWidget) &&
            !predicate(menu));
    }

    [Fact]
    public async Task InvalidTypeFilterShouldFallBackToListableTypes()
    {
        var definitions = new[]
        {
            CreateContentType("Article"),
            CreateContentType("Secret", listable: false),
        };

        var predicates = await ExecuteFilterAsync(
            new ContentTypeFilterNode("Missing"),
            definitions,
            _ => true);

        var article = new ContentItemIndex { ContentType = "Article" };
        var secret = new ContentItemIndex { ContentType = "Secret" };

        Assert.Contains(predicates, predicate =>
            predicate(article) &&
            !predicate(secret));
    }

    private static async Task<Func<ContentItemIndex, bool>[]> ExecuteFilterAsync(
        TermNode filter,
        ContentTypeDefinition[] definitions,
        Func<ContentItem, bool> canViewAll)
    {
        var definitionManager = new Mock<IContentDefinitionManager>();
        definitionManager
            .Setup(manager => manager.GetTypeDefinitionAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => definitions.FirstOrDefault(definition => definition.Name == name));
        definitionManager
            .Setup(manager => manager.ListTypeDefinitionsAsync())
            .ReturnsAsync(definitions);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object resource, IEnumerable<IAuthorizationRequirement> _) =>
                resource is ContentItem contentItem && canViewAll(contentItem)
                    ? AuthorizationResult.Success()
                    : AuthorizationResult.Failed());

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, UserId)],
            "Test"));

        using var services = new ServiceCollection()
            .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext { User = user },
            })
            .AddSingleton(authorizationService.Object)
            .AddSingleton(definitionManager.Object)
            .BuildServiceProvider();

        var predicates = new List<Expression<Func<ContentItemIndex, bool>>>();
        var query = new Mock<IQuery<ContentItem, ContentItemIndex>>();
        query
            .Setup(value => value.With<ContentItemIndex>(It.IsAny<Expression<Func<ContentItemIndex, bool>>>()))
            .Callback((Expression<Func<ContentItemIndex, bool>> predicate) => predicates.Add(predicate))
            .Returns(query.Object);
        query
            .Setup(value => value.With<ContentItemIndex>())
            .Returns(query.Object);
        query
            .Setup(value => value.Where(It.IsAny<Expression<Func<ContentItemIndex, bool>>>()))
            .Callback((Expression<Func<ContentItemIndex, bool>> predicate) => predicates.Add(predicate))
            .Returns(query.Object);
        query
            .Setup(value => value.OrderByDescending(It.IsAny<Expression<Func<ContentItemIndex, object>>>()))
            .Returns(query.Object);
        query
            .Setup(value => value.ThenBy(It.IsAny<Expression<Func<ContentItemIndex, object>>>()))
            .Returns(query.Object);

        var builder = new QueryEngineBuilder<ContentItem>();
        new DefaultContentsAdminListFilterProvider().Build(builder);

        var result = builder.Build().Parse(string.Empty);
        result.TryAddOrReplace(filter);
        await result.ExecuteAsync(new ContentQueryContext(services, query.Object));

        return [.. predicates.Select(CompilePredicate)];
    }

    private static Func<ContentItemIndex, bool> CompilePredicate(Expression<Func<ContentItemIndex, bool>> predicate)
    {
        var body = new IsInExpressionVisitor().Visit(predicate.Body);

        return Expression.Lambda<Func<ContentItemIndex, bool>>(body, predicate.Parameters).Compile();
    }

    private static ContentTypeDefinition CreateContentType(
        string name,
        string stereotype = null,
        bool listable = true)
    {
        var builder = new ContentTypeDefinitionBuilder()
            .WithName(name)
            .WithDisplayName(name)
            .Listable(listable);

        if (!string.IsNullOrEmpty(stereotype))
        {
            builder.Stereotype(stereotype);
        }

        return builder.Build();
    }

    private sealed class IsInExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name != "IsIn")
            {
                return base.VisitMethodCall(node);
            }

            var value = Visit(node.Arguments[0]);
            if (value is UnaryExpression { NodeType: ExpressionType.Convert } convert)
            {
                value = convert.Operand;
            }

            var values = Visit(node.Arguments[1]);
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(value.Type);

            return Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.Contains),
                [value.Type],
                Expression.Convert(values, enumerableType),
                value);
        }
    }
}
