# Unit and Integration Testing with Orchard Core

When developing an OrchardCore solutions, most of the times you want to unit test the most mandatory parts of your application/module.
That's why we provide a testing library, that will allow you to get started easily. We use the same library for out own OrchardCore.Tests

## How use the library

The best way to see how it works, is by check the OrchardCore.Tests project in the source code.

For integration testing, you create create a SiteContext object and configure it by using the SiteContextConfig static object.
It is mandatory to define a Startup through SiteContextConfig.WebStartupClass and a recipe to use for building the context.

