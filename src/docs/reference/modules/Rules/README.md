# Rules (`OrchardCore.Rules`)

Enabling the `OrchardCore.Rules` module allows you to implement condition based rules.

### Custom Conditions

For more intricate scenarios, you have the option to craft your own conditions. To achieve this, you'll be required to implement the following abstractions from the `OrchardCore.Rules.Abstractions` package:

- `Condition`
- `ConditionEvaluator`
- `ConditionDisplayDriver`

Afterward, proceed with registering the services as follows:

``` csharp
services.AddRule<BooleanCondition, BooleanConditionEvaluator, BooleanConditionDisplayDriver>();
```

Refer [Layers](../Layers/README.md) for more information about rules and conditions.

#### Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Iq6VbXZg0B0" frameborder="0" allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
