using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Rules;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Rules;

public class RuleManagementServiceTests
{
    [Fact]
    public void NestedConditions_UseRegisteredTypesAndPreserveIds()
    {
        var service = CreateService();
        var result = service.CreateRule(
        [
            new RuleConditionDefinition
            {
                Name = nameof(AllConditionGroup), ConditionId = "group",
                Conditions =
                [
                    new RuleConditionDefinition { Name = nameof(BooleanCondition), Properties = new JsonObject { ["value"] = true } },
                    new RuleConditionDefinition
                    {
                        Name = nameof(UrlCondition), ConditionId = "url",
                        Properties = new JsonObject { ["value"] = "/news", ["operation"] = nameof(StringStartsWithOperator) },
                    },
                ],
            },
        ], "root");

        Assert.True(result.IsValid);
        Assert.Equal("root", result.Rule.ConditionId);
        var group = Assert.IsType<AllConditionGroup>(Assert.Single(result.Rule.Conditions));
        Assert.Equal("group", group.ConditionId);
        Assert.True(Assert.IsType<BooleanCondition>(group.Conditions[0]).Value);
        Assert.NotEmpty(group.Conditions[0].ConditionId);
        var url = Assert.IsType<UrlCondition>(group.Conditions[1]);
        Assert.Equal("/news", url.Value);
        Assert.False(Assert.IsType<StringStartsWithOperator>(url.Operation).CaseSensitive);
        var roundTrip = service.CreateRule(service.Describe(result.Rule), "root");
        Assert.True(roundTrip.IsValid);
        Assert.Equal(group.Conditions[0].ConditionId, Assert.IsType<AllConditionGroup>(roundTrip.Rule.Conditions[0]).Conditions[0].ConditionId);
    }

    [Theory]
    [InlineData("BooleanCondition", "{}")]
    [InlineData("BooleanCondition", "{\"value\":\"true\"}")]
    [InlineData("BooleanCondition", "{\"value\":true,\"unexpected\":true}")]
    [InlineData("UrlCondition", "{\"value\":\"/\",\"operation\":\"MissingOperator\"}")]
    [InlineData("UrlCondition", "{\"value\":null,\"operation\":\"StringStartsWithOperator\"}")]
    [InlineData("UrlCondition", "{\"value\":\"/\",\"operation\":\"StringStartsWithOperator\",\"caseSensitive\":null}")]
    [InlineData("JavascriptCondition", "{\"script\":\"if (\"}")]
    [InlineData("JavascriptCondition", "{\"script\":\" \"}")]
    [InlineData("UnknownCondition", "{}")]
    public void InvalidConditions_AreRejectedWithoutReturningARule(string name, string properties)
    {
        var result = CreateService().CreateRule([new RuleConditionDefinition { Name = name, Properties = JsonNode.Parse(properties).AsObject() }]);
        Assert.False(result.IsValid);
        Assert.Null(result.Rule);
        Assert.Contains(result.Errors.Keys, key => key.StartsWith("conditions[0].", StringComparison.Ordinal));
    }

    [Fact]
    public void JavascriptValidation_ParsesWithoutExecuting()
    {
        var result = CreateService().CreateRule([new RuleConditionDefinition
        {
            Name = nameof(JavascriptCondition), Properties = new JsonObject { ["script"] = "throw new Error('must not execute');" },
        }]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void DuplicateIds_AndChildrenOnLeafConditions_AreRejected()
    {
        var definition = new RuleConditionDefinition { Name = nameof(HomepageCondition), ConditionId = "duplicate" };
        var service = CreateService();
        Assert.False(service.CreateRule([definition, definition]).IsValid);
        Assert.False(service.CreateRule([definition], "duplicate").IsValid);
        Assert.False(service.CreateRule([new RuleConditionDefinition { Name = nameof(HomepageCondition), Conditions = [definition] }]).IsValid);
        Assert.False(service.CreateRule(null).IsValid);
        Assert.False(service.CreateRule(Enumerable.Repeat(definition, 257).ToArray()).IsValid);
    }

    [Fact]
    public void Descriptors_AdvertiseOnlyRegisteredConditionAndOperatorContracts()
    {
        var descriptors = CreateService().GetDescriptors();
        Assert.Equal(11, descriptors.Count);
        Assert.All(descriptors, descriptor => Assert.True(descriptor.CanWrite));
        Assert.True(descriptors.Single(descriptor => descriptor.Name == nameof(AllConditionGroup)).SupportsChildren);
        var url = descriptors.Single(descriptor => descriptor.Name == nameof(UrlCondition));
        Assert.False(url.SupportsChildren);
        Assert.Equal(nameof(StringStartsWithOperator), Assert.Single(url.PropertiesSchema["properties"]["operation"]["enum"].AsArray()).GetValue<string>());
    }

    [Theory]
    [InlineData(16, true)]
    [InlineData(17, false)]
    public void NestedGroups_EnforceTheAdvertisedDepthLimit(int depth, bool valid)
    {
        var condition = new RuleConditionDefinition { Name = nameof(HomepageCondition) };
        for (var index = 0; index < depth; index++)
        {
            condition = new RuleConditionDefinition { Name = nameof(AllConditionGroup), Conditions = [condition] };
        }
        Assert.Equal(valid, CreateService().CreateRule([condition]).IsValid);
    }

    private static RuleManagementService CreateService()
    {
        IConditionFactory[] factories =
        [
            new ConditionFactory<AllConditionGroup>(), new ConditionFactory<AnyConditionGroup>(),
            new ConditionFactory<BooleanCondition>(), new ConditionFactory<JavascriptCondition>(),
            new ConditionFactory<HomepageCondition>(), new ConditionFactory<IsAuthenticatedCondition>(),
            new ConditionFactory<IsAnonymousCondition>(), new ConditionFactory<UrlCondition>(),
            new ConditionFactory<CultureCondition>(), new ConditionFactory<RoleCondition>(), new ConditionFactory<ContentTypeCondition>(),
        ];
        var ids = new Mock<IConditionIdGenerator>();
        ids.Setup(generator => generator.GenerateUniqueId(It.IsAny<Condition>()))
            .Callback<Condition>(condition => condition.ConditionId = Guid.NewGuid().ToString("N"));
        var operators = new ConditionOperatorOptions();
        operators.Operators.Add(new ConditionOperatorOption(_ => default, null, typeof(StringStartsWithOperator), new ConditionOperatorFactory<StringStartsWithOperator>()));
        return new RuleManagementService(factories, Options.Create(operators), ids.Object, new StringLocalizer<RuleManagementService>(new NullStringLocalizerFactory()));
    }
}
