# Rules (`OrchardCore.Rules`)

Enabling the `OrchardCore.Rules` module allows you to implement condition based rules.

### Custom Conditions

You may create your own conditions for more complex scenarios.

You will need to implement the abstractions found in the `OrchardCore.Rules.Abstractions` package.

- `Condition`
- `ConditionEvaluator`
- `ConditionDisplayDriver`
- Appropriate views for your condition display driver.

``` csharp
  services
    .AddScoped<IDisplayDriver<Condition>, BooleanConditionDisplayDriver>()
    .AddCondition<BooleanCondition, BooleanConditionEvaluator, ConditionFactory<BooleanCondition>>();
```

Refer [Layers](../Layers/README.md) for more information about rules and conditions.

#### Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Iq6VbXZg0B0" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
