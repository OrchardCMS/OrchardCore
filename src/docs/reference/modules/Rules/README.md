# Rules (`OrchardCore.Rules`)

Enabling the `OrchardCore.Rules` module allows you to implement condition based rules.

### Custom Conditions

You may create your own conditions for more complex scenarios.

You will need to implement the following abstractions found in the `OrchardCore.Rules.Abstractions` package.

- `Condition`
- `ConditionEvaluator`
- `ConditionDisplayDriver`
- Appropriate views for your condition display driver.

Then you can register the services like so

``` csharp
services.AddRule<BooleanCondition, BooleanConditionEvaluator, BooleanConditionDisplayDriver>();
```

Refer [Layers](../Layers/README.md) for more information about rules and conditions.

#### Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Iq6VbXZg0B0" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
