using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.AdminDashboard.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Entities;
using OrchardCore.Localization.Data;
using OrchardCore.Navigation;
using OrchardCore.Navigation.TagHelpers;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Navigation;

public class BreadcrumbTagHelperTests
{
    [Fact]
    public async Task ProcessAsync_DisabledWithoutProviders_UsesExplicitTitleWithoutEvaluatingChildren()
    {
        var services = CreateServices();
        var (helper, titleBuilder, _) = CreateHelper(services.Object);
        helper.Title = new HtmlContentString("Explicit title");
        var context = CreateContext();
        var output = new TagHelperOutput("breadcrumb", [], (_, _) =>
            throw new InvalidOperationException("Children should not be evaluated."));

        await helper.ProcessAsync(context, output);

        Assert.Equal("<h1 class=\"oc-breadcrumb-title\">Explicit title</h1>", output.Content.GetContent());
        Assert.Equal("Explicit title", GetTitle(titleBuilder));
        Assert.False(context.Items.ContainsKey(typeof(BreadcrumbItemTagHelper)));
        services.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false, null, false, "<h1 class=\"oc-breadcrumb-title\">Title</h1>", "")]
    [InlineData(false, "h2", true, "<h2 class=\"oc-breadcrumb-title\">Title</h2>", "Title")]
    [InlineData(false, "", true, "", "Title")]
    [InlineData(false, " ", true, "", "Title")]
    [InlineData(true, null, false, "<h1 class=\"oc-breadcrumb-title\">Title</h1>", "")]
    [InlineData(true, "h2", true, "<h2 class=\"oc-breadcrumb-title\">Title</h2>", "Title")]
    [InlineData(true, "", true, "", "Title")]
    [InlineData(true, " ", true, "", "Title")]
    public async Task ProcessAsync_DisabledTrail_HonorsHeadingAndPageTitle(
        bool hasProvider, string heading, bool pageTitle, string expectedHtml, string expectedTitle)
    {
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            context.Items.Clear();

            return ValueTask.CompletedTask;
        });
        var services = hasProvider ? CreateServices(provider) : CreateServices();
        var (helper, titleBuilder, _) = CreateHelper(services.Object);
        helper.Heading = heading;
        helper.PageTitle = pageTitle;

        var output = await RenderAsync(helper, ("Not the title", "end"));

        Assert.Equal(expectedHtml, output.Content.GetContent());
        Assert.Equal(expectedTitle, GetTitle(titleBuilder));
        Assert.Equal(0, calls);
        services.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, true, "h1")]
    [InlineData(null, false, "h1")]
    [InlineData("h2", true, "h2")]
    [InlineData("", true, null)]
    [InlineData(" ", true, null)]
    public async Task ProcessAsync_DisabledTrail_DoesNotResolveThrowingProviderFactoryOrEvaluateChildren(
        string heading, bool pageTitle, string expectedHeading)
    {
        var factoryCalls = 0;
        var registrations = new ServiceCollection();
        registrations.AddScoped<IBreadcrumbProvider>(_ =>
        {
            factoryCalls++;
            throw new InvalidOperationException("Provider factory should not be invoked.");
        });
        using var registeredServices = registrations.BuildServiceProvider();
        using var scope = registeredServices.CreateScope();
        var services = new Mock<IServiceProvider>(MockBehavior.Strict);
        services.Setup(value => value.GetService(typeof(IEnumerable<IBreadcrumbProvider>)))
            .Returns(() => scope.ServiceProvider.GetServices<IBreadcrumbProvider>());
        var (helper, titleBuilder, _) = CreateHelper(services.Object);
        var rawTitle = new HtmlContentString("Explicit <record> & literal &amp;");
        var title = new Mock<IHtmlContent>(MockBehavior.Strict);
        title.Setup(value => value.WriteTo(It.IsAny<TextWriter>(), HtmlEncoder.Default))
            .Callback<TextWriter, HtmlEncoder>((writer, encoder) => rawTitle.WriteTo(writer, encoder));
        helper.Title = title.Object;
        helper.Heading = heading;
        helper.PageTitle = pageTitle;
        var context = CreateContext();
        var output = new TagHelperOutput("breadcrumb", [], (_, _) =>
            throw new InvalidOperationException("Children should not be evaluated."));

        await helper.ProcessAsync(context, output);

        const string encodedTitle = "Explicit &lt;record&gt; &amp; literal &amp;amp;";
        Assert.Equal(expectedHeading is null ? "" : $"<{expectedHeading} class=\"oc-breadcrumb-title\">{encodedTitle}</{expectedHeading}>",
            output.Content.GetContent());
        Assert.Equal(pageTitle ? encodedTitle : "", GetTitle(titleBuilder));
        Assert.Equal(0, factoryCalls);
        Assert.False(context.Items.ContainsKey(typeof(BreadcrumbItemTagHelper)));
        title.Verify(value => value.WriteTo(It.IsAny<TextWriter>(), HtmlEncoder.Default), Times.Once);
        title.VerifyNoOtherCalls();
        services.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("Explicit <record> & literal &amp;", "Explicit &lt;record&gt; &amp; literal &amp;amp;")]
    [InlineData("", "")]
    public async Task ProcessAsync_DisabledTrail_IgnoresMutatingClearingAndThrowingProviders(string title, string encodedTitle)
    {
        BreadcrumbContext providerContext = null;
        var calls = 0;
        var services = CreateServices(
            new DelegateBreadcrumbProvider(context =>
            {
                providerContext = context;
                calls++;
                context.Items.Add(new BreadcrumbItem { Text = "Not the title", Position = "end" });

                return ValueTask.CompletedTask;
            }),
            new DelegateBreadcrumbProvider(context =>
            {
                providerContext = context;
                calls++;
                context.Items.Clear();

                return ValueTask.CompletedTask;
            }),
            new DelegateBreadcrumbProvider(_ => throw new InvalidOperationException("Provider should not be invoked.")));
        var (helper, titleBuilder, _) = CreateHelper(services.Object);
        helper.Title = new HtmlContentString(title);
        var tagContext = CreateContext();
        var output = new TagHelperOutput("breadcrumb", [], (_, _) =>
            throw new InvalidOperationException("Children should not be evaluated."));

        await helper.ProcessAsync(tagContext, output);

        Assert.Equal(title.Length == 0 ? "" : $"<h1 class=\"oc-breadcrumb-title\">{encodedTitle}</h1>", output.Content.GetContent());
        Assert.Equal(encodedTitle, GetTitle(titleBuilder));
        Assert.Equal(0, calls);
        Assert.Null(providerContext);
        Assert.False(tagContext.Items.ContainsKey(typeof(BreadcrumbItemTagHelper)));
        services.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ProcessAsync_DisabledWithoutHeadingOrPageTitle_DoesNotResolveProvidersOrEvaluateChildren(string heading)
    {
        var services = CreateServices(new DelegateBreadcrumbProvider(_ =>
            throw new InvalidOperationException("Providers should not be invoked.")));
        var (helper, titleBuilder, _) = CreateHelper(services.Object);
        helper.Heading = heading;
        helper.PageTitle = false;

        var context = CreateContext();
        var output = new TagHelperOutput("breadcrumb", [], (_, _) =>
            throw new InvalidOperationException("Children should not be evaluated."));

        await helper.ProcessAsync(context, output);

        Assert.True(output.IsContentModified);
        Assert.Empty(output.Content.GetContent());
        Assert.Empty(GetTitle(titleBuilder));
        Assert.False(context.Items.ContainsKey(typeof(BreadcrumbItemTagHelper)));
        services.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_DisabledWithEmptyExplicitTitle_RendersNothing()
    {
        var services = CreateServices();
        var (helper, titleBuilder, _) = CreateHelper(services.Object);
        helper.Title = new HtmlContentString("");

        var output = await RenderAsync(helper, ("Not the title", null));

        Assert.Empty(output.Content.GetContent());
        Assert.Empty(GetTitle(titleBuilder));
        services.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false, "Edit <record> & title", "Edit &lt;record&gt; &amp; title")]
    [InlineData(false, "Literal &amp; &lt;record&gt;", "Literal &amp;amp; &amp;lt;record&amp;gt;")]
    [InlineData(false, "<script>alert(\"title\")</script>", "&lt;script&gt;alert(&quot;title&quot;)&lt;/script&gt;")]
    [InlineData(true, "Edit <record> & title", "Edit &lt;record&gt; &amp; title")]
    [InlineData(true, "Literal &amp; &lt;record&gt;", "Literal &amp;amp; &amp;lt;record&amp;gt;")]
    [InlineData(true, "<script>alert(\"title\")</script>", "&lt;script&gt;alert(&quot;title&quot;)&lt;/script&gt;")]
    public async Task ProcessAsync_DisabledTrail_EncodesExplicitRawTitleExactlyOnce(bool hasProvider, string title, string encodedTitle)
    {
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Equal(title, context.Title);
            context.Items.Add(new BreadcrumbItem { Text = "Not the title" });

            return ValueTask.CompletedTask;
        });
        var services = hasProvider ? CreateServices(provider) : CreateServices();
        var (helper, titleBuilder, _) = CreateHelper(services.Object);
        helper.Title = new HtmlContentString(title);

        var output = await RenderAsync(helper);

        Assert.Equal($"<h1 class=\"oc-breadcrumb-title\">{encodedTitle}</h1>", output.Content.GetContent());
        Assert.Equal(encodedTitle, GetTitle(titleBuilder));
        Assert.Equal(0, calls);
        services.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task ProcessAsync_LocalizedHtmlTitle_FormatsArgumentsAndPreservesLiteralEntities(bool showBreadcrumb, bool hasProvider)
    {
        const string expectedTitle = "Edit <record> & value & literal &amp;";
        const string encodedTitle = "Edit &lt;record&gt; &amp; value &amp; literal &amp;amp;";
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Equal(expectedTitle, context.Title);
            Assert.Empty(context.Items);
            context.Items.Add(new BreadcrumbItem { Text = "Not the title", Position = "end" });

            return ValueTask.CompletedTask;
        });
        var services = hasProvider ? CreateServices(provider) : CreateServices();
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        helper.Title = new LocalizedHtmlString("Title", "Edit {0} &amp; {1}", false, "<record> & value", "literal &amp;");

        var output = await RenderItemsAsync(helper);

        AssertNormalizedTitle(output, display, titleBuilder, showBreadcrumb, expectedTitle, encodedTitle);
        Assert.Equal(showBreadcrumb && hasProvider ? 1 : 0, calls);
        if (showBreadcrumb)
        {
            string[] expectedTexts = hasProvider ? ["Not the title", expectedTitle] : [expectedTitle];
            Assert.Equal(expectedTexts, display.Shape.Items.Cast<BreadcrumbItemViewModel>().Select(node => node.Text));
        }
        else
        {
            services.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [InlineData(true, "Resource &amp; &lt;record&gt; &amp;amp;", "Resource & <record> &amp;", "Resource &amp; &lt;record&gt; &amp;amp;")]
    [InlineData(false, "Resource &amp; &lt;record&gt; &amp;amp;", "Resource & <record> &amp;", "Resource &amp; &lt;record&gt; &amp;amp;")]
    [InlineData(true, "<strong>Resource</strong>", "<strong>Resource</strong>", "&lt;strong&gt;Resource&lt;/strong&gt;")]
    [InlineData(false, "<strong>Resource</strong>", "<strong>Resource</strong>", "&lt;strong&gt;Resource&lt;/strong&gt;")]
    [InlineData(true, "", "", "")]
    [InlineData(false, "", "", "")]
    public async Task ProcessAsync_LocalizedHtmlResource_NormalizesEntitiesOnceAndKeepsMarkupTextOnly(
        bool showBreadcrumb, string resource, string expectedTitle, string encodedTitle)
    {
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Equal(expectedTitle, context.Title);

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        helper.Title = new LocalizedHtmlString("Title", resource);

        var output = await RenderItemsAsync(helper);

        AssertNormalizedTitle(output, display, titleBuilder, showBreadcrumb, expectedTitle, encodedTitle);
        Assert.Equal(showBreadcrumb ? 1 : 0, calls);
        if (!showBreadcrumb)
        {
            services.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [InlineData(true, true, "<strong>Title &amp; value</strong>", "&lt;strong&gt;Title &amp;amp; value&lt;/strong&gt;")]
    [InlineData(false, true, "<strong>Title &amp; value</strong>", "&lt;strong&gt;Title &amp;amp; value&lt;/strong&gt;")]
    [InlineData(true, false, "<strong>Title & value</strong>", "&lt;strong&gt;Title &amp; value&lt;/strong&gt;")]
    [InlineData(false, false, "<strong>Title & value</strong>", "&lt;strong&gt;Title &amp; value&lt;/strong&gt;")]
    public async Task ProcessAsync_HtmlContentTitle_UsesWriteToSemanticsWithoutRenderingMarkup(
        bool showBreadcrumb, bool encodeInput, string expectedTitle, string encodedTitle)
    {
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Equal(expectedTitle, context.Title);

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        helper.Title = encodeInput
            ? new HtmlContentString("<strong>Title &amp; value</strong>")
            : new HtmlString("<strong>Title &amp; value</strong>");

        var output = await RenderItemsAsync(helper);

        AssertNormalizedTitle(output, display, titleBuilder, showBreadcrumb, expectedTitle, encodedTitle);
        Assert.Equal(showBreadcrumb ? 1 : 0, calls);
        if (!showBreadcrumb)
        {
            services.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [InlineData(true, "Edit <record> & literal &amp;", "Edit &lt;record&gt; &amp; literal &amp;amp;")]
    [InlineData(false, "Edit <record> & literal &amp;", "Edit &lt;record&gt; &amp; literal &amp;amp;")]
    [InlineData(true, "", "")]
    [InlineData(false, "", "")]
    public async Task ProcessAsync_WrappedLocalizedStringValue_PreservesRawText(
        bool showBreadcrumb, string title, string encodedTitle)
    {
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Equal(title, context.Title);

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        var localizedTitle = new LocalizedString("Not the title", title);
        helper.Title = new HtmlContentString(localizedTitle.Value);

        var output = await RenderItemsAsync(helper);

        AssertNormalizedTitle(output, display, titleBuilder, showBreadcrumb, title, encodedTitle);
        Assert.Equal(showBreadcrumb ? 1 : 0, calls);
        if (!showBreadcrumb)
        {
            services.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [InlineData(true, "Edit <record> & literal &amp;", "Edit <record> & literal &amp;", "Edit &lt;record&gt; &amp; literal &amp;amp;")]
    [InlineData(false, "Edit <record> & literal &amp;", "Edit <record> & literal &amp;", "Edit &lt;record&gt; &amp; literal &amp;amp;")]
    [InlineData(true, null, "", "")]
    [InlineData(false, null, "", "")]
    public async Task ProcessAsync_WrappedDataLocalizedStringValue_PreservesRawTextAndNullAsEmpty(
        bool showBreadcrumb, string value, string expectedTitle, string encodedTitle)
    {
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Equal(expectedTitle, context.Title);

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        var title = new DataLocalizedString("Content Types", "Not the title", value);
        helper.Title = new HtmlContentString(title.Value ?? string.Empty);

        var output = await RenderItemsAsync(helper);

        AssertNormalizedTitle(output, display, titleBuilder, showBreadcrumb, expectedTitle, encodedTitle);
        Assert.Equal(showBreadcrumb ? 1 : 0, calls);
        if (showBreadcrumb)
        {
            AssertCurrentNode(Assert.Single(display.Shape.Items.Cast<BreadcrumbItemViewModel>()), expectedTitle);
        }
        else
        {
            services.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_HtmlAwareTitle_FormatsOnceAndInvokesProvidersOnlyForVisibleTrail(bool showBreadcrumb)
    {
        const string expectedTitle = "Edit <record> & literal &amp;";
        const string encodedTitle = "Edit &lt;record&gt; &amp; literal &amp;amp;";
        var localizedTitle = new LocalizedHtmlString("Title", "Edit {0} &amp; {1}", false, "<record>", "literal &amp;");
        var title = new Mock<IHtmlContent>(MockBehavior.Strict);
        title.Setup(value => value.WriteTo(It.IsAny<TextWriter>(), HtmlEncoder.Default))
            .Callback<TextWriter, HtmlEncoder>((writer, encoder) => localizedTitle.WriteTo(writer, encoder));
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            title.Verify(value => value.WriteTo(It.IsAny<TextWriter>(), HtmlEncoder.Default), Times.Once);
            Assert.Equal(expectedTitle, context.Title);

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider, provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        helper.Title = title.Object;

        var output = await RenderItemsAsync(helper);

        AssertNormalizedTitle(output, display, titleBuilder, showBreadcrumb, expectedTitle, encodedTitle);
        Assert.Equal(encodedTitle, GetTitle(titleBuilder));
        Assert.Equal(showBreadcrumb ? 2 : 0, calls);
        title.Verify(value => value.WriteTo(It.IsAny<TextWriter>(), HtmlEncoder.Default), Times.Once);
        title.VerifyNoOtherCalls();
        if (!showBreadcrumb)
        {
            services.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ProcessAsync_DisabledWithoutOutput_DoesNotFormatHtmlAwareTitle(string heading)
    {
        var title = new Mock<IHtmlContent>(MockBehavior.Strict);
        var services = CreateServices(new DelegateBreadcrumbProvider(_ =>
            throw new InvalidOperationException("Providers should not be invoked.")));
        var (helper, titleBuilder, _) = CreateHelper(services.Object);
        helper.Title = title.Object;
        helper.Heading = heading;
        helper.PageTitle = false;
        var context = CreateContext();
        var output = new TagHelperOutput("breadcrumb", [], (_, _) =>
            throw new InvalidOperationException("Children should not be evaluated."));

        await helper.ProcessAsync(context, output);

        Assert.True(output.IsContentModified);
        Assert.Empty(output.Content.GetContent());
        Assert.Empty(GetTitle(titleBuilder));
        Assert.False(context.Items.ContainsKey(typeof(BreadcrumbItemTagHelper)));
        title.VerifyNoOtherCalls();
        services.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("Title")]
    [InlineData("")]
    public async Task ProcessAsync_VisibleWithoutAncestors_RendersExplicitTitleNode(string title)
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true);
        helper.Title = new HtmlContentString(title);

        var output = await RenderAsync(helper);

        var current = Assert.Single(display.Shape.Items.Cast<BreadcrumbItemViewModel>());
        AssertCurrentNode(current, title);
        Assert.Equal(title, display.Shape.GetProperty<string>("Title"));
        Assert.Equal(title, GetTitle(titleBuilder));
        Assert.Equal("rendered trail", output.Content.GetContent());
        VerifyNoLinkServicesResolved(services);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_NullTitle_RejectsMissingRequiredTitle(bool showBreadcrumb)
    {
        var services = new Mock<IServiceProvider>(MockBehavior.Strict);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        helper.Title = null;
        helper.Heading = "";
        helper.PageTitle = false;
        var output = new TagHelperOutput("breadcrumb", [], (_, _) =>
            throw new InvalidOperationException("Children should not be evaluated."));

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => helper.ProcessAsync(CreateContext(), output));

        Assert.Equal(nameof(BreadcrumbTagHelper.Title), exception.ParamName);
        Assert.Empty(GetTitle(titleBuilder));
        services.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(nameof(BreadcrumbTagHelper.Title), "title", typeof(IHtmlContent))]
    [InlineData(nameof(BreadcrumbTagHelper.Name), "name", typeof(string))]
    public void Attributes_TitleAndName_BindToExpectedNames(string propertyName, string attributeName, Type propertyType)
    {
        var property = typeof(BreadcrumbTagHelper).GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.Equal(attributeName, property.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name);
    }

    [Theory]
    [InlineData(nameof(BreadcrumbContext.Name))]
    [InlineData(nameof(BreadcrumbContext.Title))]
    [InlineData(nameof(BreadcrumbContext.Items))]
    [InlineData(nameof(BreadcrumbContext.ViewContext))]
    [InlineData(nameof(BreadcrumbContext.ShowTrail))]
    public void BreadcrumbContext_Metadata_ExposesReadOnlyProperties(string propertyName)
    {
        var property = typeof(BreadcrumbContext).GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.True(property.CanRead);
        Assert.Null(property.SetMethod);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BreadcrumbContext_Constructor_PreservesExplicitMetadataAndAncestorList(bool showTrail)
    {
        var items = new List<BreadcrumbItem>();
        var viewContext = new ViewContext();
        var context = new BreadcrumbContext("RecordsEdit", "Literal &amp;", items, viewContext, showTrail);

        Assert.Equal("RecordsEdit", context.Name);
        Assert.Equal("Literal &amp;", context.Title);
        Assert.Equal(showTrail, context.ShowTrail);
        Assert.Same(viewContext, context.ViewContext);
        Assert.Same(items, context.Items);
        Assert.Empty(context.Items);
        Assert.Equal("", new BreadcrumbContext("RecordsEdit", "", items, viewContext, showTrail).Title);
    }

    [Theory]
    [InlineData(false, "Edit <record> & title", "Edit &lt;record&gt; &amp; title")]
    [InlineData(false, "Literal &amp; &lt;record&gt;", "Literal &amp;amp; &amp;lt;record&amp;gt;")]
    [InlineData(true, "Edit <record> & title", "Edit &lt;record&gt; &amp; title")]
    [InlineData(true, "Literal &amp; &lt;record&gt;", "Literal &amp;amp; &amp;lt;record&amp;gt;")]
    public async Task ProcessAsync_VisibleTrail_PreservesRawTitleAndDecodesInlineAncestorsOnce(
        bool hasProvider, string title, string encodedTitle)
    {
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Equal(title, context.Title);
            Assert.Equal("Records <list> & literal &amp;", Assert.Single(context.Items).Text);

            return ValueTask.CompletedTask;
        });
        var services = hasProvider ? CreateServices(provider) : CreateServices();
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true);
        helper.Title = new HtmlContentString(title);

        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper(), "Records &lt;list&gt; &amp; literal &amp;amp;"));

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal(["Records <list> & literal &amp;", title], nodes.Select(node => node.Text));
        Assert.Equal(title, display.Shape.GetProperty<string>("Title"));
        Assert.Equal(encodedTitle, GetTitle(titleBuilder));
        Assert.Equal(hasProvider ? 1 : 0, calls);
        AssertCurrentNode(nodes[^1], title);
        VerifyNoLinkServicesResolved(services);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_VisibleTrail_ResolvesProvidersAfterChildrenRegardlessOfAdminSetting(bool isAdmin)
    {
        var childrenEvaluated = false;
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.True(childrenEvaluated);
            Assert.True(context.ShowTrail);
            Assert.Equal("Parent", Assert.Single(context.Items).Text);
            context.Items.Add(new BreadcrumbItem { Text = "Provider ancestor", Url = "/provider" });

            return ValueTask.CompletedTask;
        });
        IBreadcrumbProvider[] providers = [provider];
        var services = CreateServices(providers);
        services.Setup(value => value.GetService(typeof(IEnumerable<IBreadcrumbProvider>)))
            .Callback(() => Assert.True(childrenEvaluated))
            .Returns(providers);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, siteService) = CreateHelper(services.Object, showBreadcrumb: isAdmin, isAdmin: isAdmin);
        var tagContext = CreateContext();
        var output = new TagHelperOutput("breadcrumb", [], async (_, _) =>
        {
            var child = new BreadcrumbItemTagHelper { Url = "/parent" };
            var childOutput = new TagHelperOutput("breadcrumb-item", [], (_, _) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent().SetHtmlContent("Parent")));
            await child.ProcessAsync(tagContext, childOutput);
            childrenEvaluated = true;

            return new DefaultTagHelperContent();
        });

        await helper.ProcessAsync(tagContext, output);

        Assert.Equal(1, calls);
        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal(["Parent", "Provider ancestor", "Title"], nodes.Select(node => node.Text));
        Assert.Equal(["/parent", "/provider", null], nodes.Select(node => node.Href));
        AssertCurrentNode(nodes[^1], "Title");
        Assert.Equal("Title", GetTitle(titleBuilder));
        Assert.Equal(isAdmin ? "h1" : null, display.Shape.GetProperty<string>("Heading"));
        services.Verify(value => value.GetService(typeof(IEnumerable<IBreadcrumbProvider>)), Times.Once);
        siteService.Verify(value => value.GetSiteSettingsAsync(), isAdmin ? Times.Once() : Times.Never());
        VerifyNoLinkServicesResolved(services);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_VisibleTrail_BuildsAuthorizedRoutesAndShapes(bool isAdmin)
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var permission = new Permission("ManageRecords", "Manage records");
        var permissions = new Mock<IPermissionService>(MockBehavior.Strict);
        permissions.Setup(value => value.FindByNameAsync(permission.Name)).ReturnsAsync(permission);
        services.Setup(value => value.GetService(typeof(IPermissionService))).Returns(permissions.Object);
        var resource = new object();
        var authorization = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), resource, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        services.Setup(value => value.GetService(typeof(IAuthorizationService))).Returns(authorization.Object);
        var urlHelper = new Mock<IUrlHelper>(MockBehavior.Strict);
        urlHelper.Setup(value => value.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns((UrlRouteContext context) =>
            {
                var values = Assert.IsType<RouteValueDictionary>(context.Values);
                Assert.Equal("Index", values["action"]);
                Assert.Equal("Admin", values["controller"]);
                Assert.Equal("My.Module", values["area"]);
                Assert.Equal("42", values["id"]);

                return "/tenant/records/42";
            });
        var urlFactory = new Mock<IUrlHelperFactory>(MockBehavior.Strict);
        urlFactory.Setup(value => value.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelper.Object);
        services.Setup(value => value.GetService(typeof(IUrlHelperFactory))).Returns(urlFactory.Object);
        // The admin setting must not affect a front-end trail.
        var (helper, titleBuilder, siteService) = CreateHelper(services.Object, showBreadcrumb: isAdmin, isAdmin: isAdmin);
        helper.Title = new HtmlContentString("Edit <Record>");
        var parent = new BreadcrumbItemTagHelper
        {
            Id = "Records",
            Action = "Index",
            Controller = "Admin",
            Area = "My.Module",
            RouteValues = new Dictionary<string, string> { ["id"] = "42" },
            PermissionName = permission.Name,
            Resource = resource,
            Url = "/ignored-for-route",
        };

        var output = await RenderItemsAsync(helper, (parent, "Records &amp; More"));

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal(["Records & More", "Edit <Record>"], nodes.Select(node => node.Text));
        Assert.Equal("/tenant/records/42", nodes[0].Href);
        Assert.False(nodes[0].IsCurrent);
        AssertCurrentNode(nodes[1], "Edit <Record>");
        Assert.Equal("RecordsEdit", nodes[0].Name);
        Assert.Equal("Records", nodes[0].Item.Id);
        Assert.Same(resource, nodes[0].Item.Resource);
        Assert.Equal("Edit &lt;Record&gt;", GetTitle(titleBuilder));
        Assert.Equal("Edit <Record>", display.Shape.GetProperty<string>("Title"));
        Assert.Equal(isAdmin ? "h1" : null, display.Shape.GetProperty<string>("Heading"));
        Assert.Equal(isAdmin ? "DetailAdmin" : "Detail", display.Shape.Metadata.DisplayType);
        Assert.Equal("rendered trail", output.Content.GetContent());
        permissions.Verify(value => value.FindByNameAsync(permission.Name), Times.Once);
        authorization.VerifyAll();
        urlHelper.Verify(value => value.RouteUrl(It.IsAny<UrlRouteContext>()), Times.Once);
        siteService.Verify(value => value.GetSiteSettingsAsync(), isAdmin ? Times.Once() : Times.Never());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_DeniedLink_KeepsTextWithoutGeneratingUrl(bool linkEnabled)
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var permission = new Permission("ManageRecords", "Manage records");
        var permissions = new Mock<IPermissionService>(MockBehavior.Strict);
        var authorization = new Mock<IAuthorizationService>(MockBehavior.Strict);

        if (linkEnabled)
        {
            permissions.Setup(value => value.FindByNameAsync(permission.Name)).ReturnsAsync(permission);
            services.Setup(value => value.GetService(typeof(IPermissionService))).Returns(permissions.Object);
            authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Failed());
            services.Setup(value => value.GetService(typeof(IAuthorizationService))).Returns(authorization.Object);
        }

        var (helper, _, _) = CreateHelper(services.Object, showBreadcrumb: true);
        helper.Title = new HtmlContentString("Edit");
        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper { Action = "Index", PermissionName = permission.Name, LinkEnabled = linkEnabled }, "Records"));

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal(["Records", "Edit"], nodes.Select(node => node.Text));
        Assert.All(nodes, node => Assert.Null(node.Href));
        AssertCurrentNode(nodes[^1], "Edit");
        permissions.Verify(value => value.FindByNameAsync(permission.Name), linkEnabled ? Times.Once() : Times.Never());
        authorization.Verify(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>()),
            linkEnabled ? Times.Once() : Times.Never());
        services.Verify(value => value.GetService(typeof(IPermissionService)), linkEnabled ? Times.Once() : Times.Never());
        services.Verify(value => value.GetService(typeof(IAuthorizationService)), linkEnabled ? Times.Once() : Times.Never());
        services.Verify(value => value.GetService(typeof(IUrlHelperFactory)), Times.Never);
    }

    [Theory]
    [InlineData("~/Admin", "/tenant/Admin")]
    [InlineData("Admin", "/tenant/Admin")]
    [InlineData("/Admin", "/Admin")]
    [InlineData("https://example.org/records", "https://example.org/records")]
    public async Task ProcessAsync_UrlAncestor_RespectsPathBase(string url, string expected)
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var (helper, _, _) = CreateHelper(services.Object, showBreadcrumb: true);
        helper.ViewContext.HttpContext.Request.PathBase = "/tenant";

        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper { Url = url }, "Records"));

        Assert.Equal(expected, display.Shape.Items.Cast<BreadcrumbItemViewModel>().First().Href);
        AssertCurrentNode(display.Shape.Items.Cast<BreadcrumbItemViewModel>().Last(), "Title");
        VerifyNoLinkServicesResolved(services);
    }

    [Theory]
    [InlineData("10", "2", "Second", "First")]
    [InlineData("1", "1", "First", "Second")]
    [InlineData("end", "after", "Second", "First")]
    [InlineData(null, null, "First", "Second")]
    [InlineData("start", null, "First", "Second")]
    [InlineData(null, "", "First", "Second")]
    [InlineData(" ", null, "Second", "First")]
    [InlineData("end", "end", "First", "Second")]
    public async Task ProcessAsync_VisibleTrail_OrdersAncestorsStablyBeforeExplicitTitle(string firstPosition, string secondPosition, string firstText, string secondText)
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true);

        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper { Position = firstPosition, Url = "/first" }, "First"),
            (new BreadcrumbItemTagHelper { Position = secondPosition, Url = "/second" }, "Second"));

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal([firstText, secondText, "Title"], nodes.Select(node => node.Text));
        Assert.All(nodes.Take(2), node =>
        {
            Assert.False(node.IsCurrent);
            Assert.Equal(node.Text == "First" ? "/first" : "/second", node.Href);
        });
        AssertCurrentNode(nodes[^1], "Title");
        Assert.Equal("Title", GetTitle(titleBuilder));
    }

    [Theory]
    [InlineData(true, null, true, "h1", "Title")]
    [InlineData(false, null, true, null, "Title")]
    [InlineData(true, "h2", false, "h2", "")]
    [InlineData(false, "h2", false, "h2", "")]
    [InlineData(true, "", true, null, "Title")]
    [InlineData(true, " ", false, null, "")]
    public async Task ProcessAsync_VisibleTrail_HonorsHeadingAndPageTitleIndependently(
        bool isAdmin, string heading, bool pageTitle, string expectedHeading, string expectedPageTitle)
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true, isAdmin: isAdmin);
        helper.Heading = heading;
        helper.PageTitle = pageTitle;

        var output = await RenderItemsAsync(helper);

        Assert.Equal(expectedHeading, display.Shape.GetProperty<string>("Heading"));
        Assert.Equal("Title", display.Shape.GetProperty<string>("Title"));
        Assert.Equal(expectedPageTitle, GetTitle(titleBuilder));
        AssertCurrentNode(Assert.Single(display.Shape.Items.Cast<BreadcrumbItemViewModel>()), "Title");
        Assert.Equal("rendered trail", output.Content.GetContent());
    }

    [Theory]
    [InlineData(true, null, "DetailAdmin")]
    [InlineData(false, null, "Detail")]
    [InlineData(true, " ", "DetailAdmin")]
    [InlineData(false, "", "Detail")]
    [InlineData(true, "Summary", "Summary")]
    [InlineData(false, "Summary", "Summary")]
    public async Task ProcessAsync_TitleNode_UsesAutomaticIdAndNameBasedAlternates(
        bool isAdmin, string displayType, string expectedDisplayType)
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var (helper, _, _) = CreateHelper(services.Object, showBreadcrumb: true, isAdmin: isAdmin);
        helper.Name = isAdmin ? "RecordsEdit" : "PublicRecords";
        helper.DisplayType = displayType;

        await RenderItemsAsync(helper, (new BreadcrumbItemTagHelper { Id = "Records" }, "Records"));

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        var current = nodes[^1];
        AssertCurrentNode(current, "Title");
        Assert.Equal(helper.Name, current.Name);
        Assert.Same(display.Shape, current.Breadcrumb);
        Assert.Equal([0, 1], nodes.Select(node => node.Level));
        Assert.Equal(expectedDisplayType, display.Shape.Metadata.DisplayType);
        Assert.All(nodes, node => Assert.Equal(expectedDisplayType, node.Metadata.DisplayType));
        Assert.Equal(
            [
                $"BreadcrumbItem__{helper.Name}",
                "BreadcrumbItem__Title",
                $"BreadcrumbItem__{helper.Name}__Title",
                $"BreadcrumbItem_{expectedDisplayType}",
                $"BreadcrumbItem_{expectedDisplayType}__{helper.Name}",
                $"BreadcrumbItem_{expectedDisplayType}__Title",
                $"BreadcrumbItem_{expectedDisplayType}__{helper.Name}__Title",
            ],
            BreadcrumbAlternatesFactory.GetBreadcrumbItemAlternates(current.Name, current.Item.Id, current.Metadata.DisplayType));
    }

    [Fact]
    public async Task ProcessAsync_VisibleProviders_UpdateAncestorsInRegistrationOrderWithoutChangingExplicitTitle()
    {
        BreadcrumbContext firstContext = null;
        var firstProvider = new DelegateBreadcrumbProvider(async context =>
        {
            await Task.Yield();
            firstContext = context;
            Assert.Equal("RecordsEdit", context.Name);
            Assert.Equal("Explicit <record> & title", context.Title);
            Assert.True(context.ShowTrail);
            Assert.Equal(["Parent", "Removed", "Original"], context.Items.Select(item => item.Text));
            Assert.Equal("NeverResolve", context.Items[1].PermissionName);
            Assert.Equal("Index", context.Items[1].RouteValues["action"]);
            context.Items.RemoveAt(1);
            context.Items[1].Text = "Updated";
            context.Items.Insert(0, new BreadcrumbItem { Id = "Root", Text = "Root", Position = "start", Url = "/root" });
        });
        var secondProvider = new DelegateBreadcrumbProvider(context =>
        {
            Assert.Same(firstContext, context);
            Assert.Equal(["Root", "Parent", "Updated"], context.Items.Select(item => item.Text));
            context.Items[1].Url = "/changed-parent";
            context.Items[2] = new BreadcrumbItem
            {
                Id = "Injected",
                Text = "Injected <ancestor> & text",
                Position = "end",
                Url = "/disabled",
                PermissionName = "NeverResolve",
                LinkEnabled = false,
                IsCurrent = true,
                Href = "/stale",
            };
            context.Items.Reverse();

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(firstProvider, secondProvider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true);
        helper.Title = new HtmlContentString("Explicit <record> & title");

        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper { Position = "10" }, "Parent"),
            (new BreadcrumbItemTagHelper { Action = "Index", PermissionName = "NeverResolve" }, "Removed"),
            (new BreadcrumbItemTagHelper(), "Original"));

        Assert.Same(helper.ViewContext, firstContext.ViewContext);
        Assert.Equal("Explicit &lt;record&gt; &amp; title", GetTitle(titleBuilder));

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal(["Root", "Parent", "Injected <ancestor> & text", "Explicit <record> & title"], nodes.Select(node => node.Text));
        Assert.Equal(["/root", "/changed-parent", null, null], nodes.Select(node => node.Href));
        Assert.Equal([false, false, false, true], nodes.Select(node => node.IsCurrent));
        AssertCurrentNode(nodes[^1], "Explicit <record> & title");
        Assert.Equal("Explicit <record> & title", display.Shape.GetProperty<string>("Title"));

        VerifyNoLinkServicesResolved(services);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_ProviderOnlyTrail_UsesAncestorsOnlyWhenVisible(bool showBreadcrumb)
    {
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(async context =>
        {
            await Task.Yield();
            calls++;
            Assert.Empty(context.Items);
            Assert.Equal("Title", context.Title);
            context.Items.Add(new BreadcrumbItem { Text = "Records", Url = "/records" });
            context.Items.Add(new BreadcrumbItem { Text = "Provider <ancestor>", Position = "end", Url = "/provider" });
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);

        var output = await RenderItemsAsync(helper);

        Assert.Equal(showBreadcrumb ? 1 : 0, calls);
        Assert.Equal("Title", GetTitle(titleBuilder));

        if (showBreadcrumb)
        {
            var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
            Assert.Equal(["Records", "Provider <ancestor>", "Title"], nodes.Select(node => node.Text));
            Assert.Equal(["/records", "/provider", null], nodes.Select(node => node.Href));
            Assert.Equal([false, false, true], nodes.Select(node => node.IsCurrent));
            AssertCurrentNode(nodes[^1], "Title");
        }
        else
        {
            Assert.Equal("<h1 class=\"oc-breadcrumb-title\">Title</h1>", output.Content.GetContent());
            Assert.Null(display.Shape);
            services.VerifyNoOtherCalls();
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Title")]
    public async Task ProcessAsync_ProviderSuppliesCurrentNode_StillAppendsUnlinkedExplicitTitle(string ancestorId)
    {
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            Assert.Empty(context.Items);
            context.Items.Add(new BreadcrumbItem
            {
                Id = ancestorId,
                Text = context.Title,
                Position = "end",
                Url = "/ancestor",
                IsCurrent = true,
                Href = "/stale",
            });

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true);

        await RenderItemsAsync(helper);

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal(["Title", "Title"], nodes.Select(node => node.Text));
        Assert.Equal([ancestorId, "Title"], nodes.Select(node => node.Item.Id));
        Assert.False(nodes[0].IsCurrent);
        Assert.False(nodes[0].Item.IsCurrent);
        Assert.Equal("/ancestor", nodes[0].Href);
        AssertCurrentNode(nodes[^1], "Title");
        Assert.NotSame(nodes[0].Item, nodes[1].Item);
        Assert.Equal("Title", GetTitle(titleBuilder));
        VerifyNoLinkServicesResolved(services);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_InlineChildrenAndProviders_UseSharedContextOnlyForVisibleTrail(bool showBreadcrumb)
    {
        BreadcrumbContext childContext = null;
        var resource = new object();
        var calls = 0;
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Same(childContext, context);
            Assert.Equal("RecordsEdit", context.Name);
            Assert.Equal("Title", context.Title);
            Assert.True(context.ShowTrail);
            Assert.Equal(["Records & More", "Other"], context.Items.Select(item => item.Text));
            var ancestor = context.Items[0];
            Assert.Equal("Records", ancestor.Id);
            Assert.Equal("end", ancestor.Position);
            Assert.Equal("ManageRecords", ancestor.PermissionName);
            Assert.Same(resource, ancestor.Resource);
            Assert.False(ancestor.LinkEnabled);
            Assert.False(ancestor.IsCurrent);
            Assert.Null(ancestor.Href);
            Assert.Equal("Index", ancestor.RouteValues["action"]);
            Assert.Equal("Admin", ancestor.RouteValues["controller"]);
            Assert.Equal("My.Module", ancestor.RouteValues["area"]);
            Assert.Equal("42", ancestor.RouteValues["id"]);
            Assert.Equal("/other", context.Items[1].Url);

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        var tagContext = CreateContext();
        var output = new TagHelperOutput("breadcrumb", [], async (_, _) =>
        {
            childContext = Assert.IsType<BreadcrumbContext>(tagContext.Items[typeof(BreadcrumbItemTagHelper)]);
            Assert.Same(helper.ViewContext, childContext.ViewContext);
            Assert.Empty(childContext.Items);
            (BreadcrumbItemTagHelper Helper, string Text)[] children =
            [
                (Helper: new BreadcrumbItemTagHelper
                {
                    Id = "Records",
                    Position = "end",
                    PermissionName = "ManageRecords",
                    Resource = resource,
                    LinkEnabled = false,
                    Action = "Index",
                    Controller = "Admin",
                    Area = "My.Module",
                    RouteValues = new Dictionary<string, string> { ["id"] = "42" },
                }, Text: "Records &amp; More"),
                (Helper: new BreadcrumbItemTagHelper { Url = "/other" }, Text: "Other"),
            ];

            foreach (var (child, text) in children)
            {
                var childOutput = new TagHelperOutput("breadcrumb-item", [], (_, _) =>
                    Task.FromResult<TagHelperContent>(new DefaultTagHelperContent().SetHtmlContent(text)));
                await child.ProcessAsync(tagContext, childOutput);
                Assert.Empty(childOutput.Content.GetContent());
            }

            return new DefaultTagHelperContent();
        });

        await helper.ProcessAsync(tagContext, output);

        Assert.Equal(showBreadcrumb ? 1 : 0, calls);
        Assert.Equal("Title", GetTitle(titleBuilder));
        if (showBreadcrumb)
        {
            var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
            Assert.Equal(["Other", "Records & More", "Title"], nodes.Select(node => node.Text));
            AssertCurrentNode(nodes[^1], "Title");
        }
        else
        {
            Assert.Equal("<h1 class=\"oc-breadcrumb-title\">Title</h1>", output.Content.GetContent());
            Assert.Null(display.Shape);
            Assert.Null(childContext);
            Assert.False(tagContext.Items.ContainsKey(typeof(BreadcrumbItemTagHelper)));
            services.VerifyNoOtherCalls();
        }

        VerifyNoLinkServicesResolved(services);
    }

    [Fact]
    public async Task ProcessAsync_VisibleProviderClearsItems_CannotRemoveExplicitTitle()
    {
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            Assert.Single(context.Items);
            context.Items.Clear();

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true);

        var output = await RenderAsync(helper, ("Removed", null));

        Assert.Equal("Title", GetTitle(titleBuilder));
        AssertCurrentNode(Assert.Single(display.Shape.Items.Cast<BreadcrumbItemViewModel>()), "Title");
        Assert.Equal("Title", display.Shape.GetProperty<string>("Title"));
        Assert.Equal("rendered trail", output.Content.GetContent());
        VerifyNoLinkServicesResolved(services);
    }

    [Fact]
    public async Task ProcessAsync_VisibleProviderChangesAncestorTextAndPosition_PreservesExplicitEmptyTitle()
    {
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            Assert.Equal("", context.Title);
            context.Items[0].Position = "end";
            context.Items[0].Text = "Not the title";

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true);
        helper.Title = new HtmlContentString("");

        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper(), "First"),
            (new BreadcrumbItemTagHelper(), "Second"));

        Assert.Empty(GetTitle(titleBuilder));

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal(["Second", "Not the title", ""], nodes.Select(node => node.Text));
        AssertCurrentNode(nodes[^1], "");
        Assert.Equal("", display.Shape.GetProperty<string>("Title"));
    }

    [Fact]
    public async Task ProcessAsync_VisibleProviderThrows_PropagatesFailureBeforeRenderingOrRegisteringTitle()
    {
        var exception = new InvalidOperationException("Provider failed.");
        var services = CreateServices(new DelegateBreadcrumbProvider(_ => throw exception));
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: true);

        Assert.Same(exception, await Assert.ThrowsAsync<InvalidOperationException>(() => RenderItemsAsync(helper)));
        Assert.Empty(GetTitle(titleBuilder));
        VerifyOnlyProvidersResolved(services);
    }

    [Fact]
    public async Task ProcessAsync_UnknownPermission_PreservesAncestorLink()
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var permissions = new Mock<IPermissionService>(MockBehavior.Strict);
        permissions.Setup(value => value.FindByNameAsync("Unknown")).ReturnsAsync((Permission)null);
        services.Setup(value => value.GetService(typeof(IPermissionService))).Returns(permissions.Object);
        var (helper, _, _) = CreateHelper(services.Object, showBreadcrumb: true);

        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper { Url = "/records", PermissionName = "Unknown" }, "Records"));

        Assert.Equal("/records", display.Shape.Items.Cast<BreadcrumbItemViewModel>().First().Href);
        permissions.VerifyAll();
        services.Verify(value => value.GetService(typeof(IAuthorizationService)), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_ProviderChangesPermission_ResolvesOnlyFinalAncestorPermission()
    {
        var permission = new Permission("FinalPermission", "Final permission");
        var provider = new DelegateBreadcrumbProvider(context =>
        {
            var ancestor = Assert.Single(context.Items);
            Assert.Equal("OriginalPermission", ancestor.PermissionName);
            ancestor.PermissionName = permission.Name;

            return ValueTask.CompletedTask;
        });
        var services = CreateServices(provider);
        var display = ConfigureRendering(services);
        var permissions = new Mock<IPermissionService>(MockBehavior.Strict);
        permissions.Setup(value => value.FindByNameAsync(permission.Name)).ReturnsAsync(permission);
        services.Setup(value => value.GetService(typeof(IPermissionService))).Returns(permissions.Object);
        var authorization = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        services.Setup(value => value.GetService(typeof(IAuthorizationService))).Returns(authorization.Object);
        var (helper, _, _) = CreateHelper(services.Object, showBreadcrumb: true);

        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper { Url = "/records", PermissionName = "OriginalPermission" }, "Records"));

        Assert.Equal("/records", display.Shape.Items.Cast<BreadcrumbItemViewModel>().First().Href);
        permissions.Verify(value => value.FindByNameAsync(permission.Name), Times.Once);
        permissions.VerifyNoOtherCalls();
        authorization.VerifyAll();
    }

    [Fact]
    public async Task ProcessAsync_AncestorWithoutLink_DoesNotResolveNamedPermission()
    {
        var services = CreateServices();
        var display = ConfigureRendering(services);
        var (helper, _, _) = CreateHelper(services.Object, showBreadcrumb: true);

        await RenderItemsAsync(helper,
            (new BreadcrumbItemTagHelper { PermissionName = "NeverResolve" }, "Records"));

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        Assert.Equal(["Records", "Title"], nodes.Select(node => node.Text));
        Assert.All(nodes, node => Assert.Null(node.Href));
        VerifyNoLinkServicesResolved(services);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task ProcessAsync_DashboardFeature_AddsAuthorizedAdminRootOnly(bool isAdmin, bool authorized)
    {
        var services = CreateServices(CreateDashboardProvider());
        var display = ConfigureRendering(services);
        var (helper, _, _) = CreateHelper(services.Object, showBreadcrumb: true, isAdmin: isAdmin);
        helper.Title = new HtmlContentString("Records");
        helper.ViewContext.HttpContext.Request.PathBase = "/tenant";
        var authorization = new Mock<IAuthorizationService>(MockBehavior.Strict);
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        services.Setup(value => value.GetService(typeof(IAuthorizationService))).Returns(authorization.Object);

        await RenderItemsAsync(helper);

        var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
        string[] expectedTexts = isAdmin ? ["Dashboard", "Records"] : ["Records"];
        Assert.Equal(expectedTexts, nodes.Select(node => node.Text));
        if (isAdmin)
        {
            Assert.Equal("Dashboard", nodes[0].Item.Id);
            Assert.Equal("start", nodes[0].Item.Position);
            Assert.Same(global::OrchardCore.AdminDashboard.Permissions.AccessAdminDashboard, Assert.Single(nodes[0].Item.Permissions));
            Assert.False(nodes[0].IsCurrent);
            Assert.Equal(authorized ? "/tenant/Backend" : null, nodes[0].Href);
        }

        AssertCurrentNode(nodes[^1], "Records");
        services.Verify(value => value.GetService(typeof(IAuthorizationService)), isAdmin ? Times.Once() : Times.Never());
        services.Verify(value => value.GetService(typeof(IPermissionService)), Times.Never);
    }

    [Theory]
    [InlineData(true, "Title")]
    [InlineData(false, "Title")]
    [InlineData(true, "Dashboard")]
    [InlineData(false, "Dashboard")]
    public async Task ProcessAsync_DashboardProvider_AddsAncestorWithoutReplacingExplicitTitle(bool showBreadcrumb, string title)
    {
        var services = CreateServices(CreateDashboardProvider());
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        helper.Title = new HtmlContentString(title);
        var authorization = new Mock<IAuthorizationService>(MockBehavior.Strict);

        if (showBreadcrumb)
        {
            authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());
            services.Setup(value => value.GetService(typeof(IAuthorizationService))).Returns(authorization.Object);
        }

        var output = await RenderItemsAsync(helper);

        Assert.Equal(title, GetTitle(titleBuilder));

        if (showBreadcrumb)
        {
            var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
            Assert.Equal(["Dashboard", title], nodes.Select(node => node.Text));
            Assert.Equal("Dashboard", nodes[0].Item.Id);
            Assert.Equal("/Backend", nodes[0].Href);
            Assert.False(nodes[0].IsCurrent);
            AssertCurrentNode(nodes[^1], title);
        }
        else
        {
            Assert.Equal($"<h1 class=\"oc-breadcrumb-title\">{title}</h1>", output.Content.GetContent());
            Assert.Null(display.Shape);
            services.VerifyNoOtherCalls();
        }

        services.Verify(value => value.GetService(typeof(IAuthorizationService)), showBreadcrumb ? Times.Once() : Times.Never());
        services.Verify(value => value.GetService(typeof(IPermissionService)), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_DashboardAncestorAlreadyDeclared_DoesNotDuplicateRoot(bool showBreadcrumb)
    {
        var calls = 0;
        var services = CreateServices(CreateDashboardProvider(), new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            var ancestor = Assert.Single(context.Items);
            Assert.Equal("Dashboard", ancestor.Id);
            Assert.Equal("My Dashboard", ancestor.Text);

            return ValueTask.CompletedTask;
        }));
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);

        var output = await RenderItemsAsync(helper, (new BreadcrumbItemTagHelper { Id = "Dashboard" }, "My Dashboard"));

        Assert.Equal(showBreadcrumb ? 1 : 0, calls);
        Assert.Equal("Title", GetTitle(titleBuilder));

        if (showBreadcrumb)
        {
            var nodes = display.Shape.Items.Cast<BreadcrumbItemViewModel>().ToArray();
            Assert.Equal(["My Dashboard", "Title"], nodes.Select(node => node.Text));
            Assert.False(nodes[0].IsCurrent);
            AssertCurrentNode(nodes[^1], "Title");
        }
        else
        {
            Assert.Equal("<h1 class=\"oc-breadcrumb-title\">Title</h1>", output.Content.GetContent());
            services.VerifyNoOtherCalls();
        }

        VerifyNoLinkServicesResolved(services);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessAsync_DashboardNameWithLocalizedTitle_DoesNotAddDashboardAncestor(bool showBreadcrumb)
    {
        var calls = 0;
        var services = CreateServices(CreateDashboardProvider(), new DelegateBreadcrumbProvider(context =>
        {
            calls++;
            Assert.Equal("Dashboard", context.Name);
            Assert.Equal("My workspace & reports", context.Title);
            Assert.Empty(context.Items);

            return ValueTask.CompletedTask;
        }));
        var display = ConfigureRendering(services);
        var (helper, titleBuilder, _) = CreateHelper(services.Object, showBreadcrumb: showBreadcrumb);
        helper.Title = new LocalizedHtmlString("Dashboard", "My workspace &amp; reports");
        helper.Name = "Dashboard";

        var output = await RenderItemsAsync(helper);

        Assert.Equal(showBreadcrumb ? 1 : 0, calls);
        Assert.Equal("My workspace &amp; reports", GetTitle(titleBuilder));

        if (showBreadcrumb)
        {
            var current = Assert.Single(display.Shape.Items.Cast<BreadcrumbItemViewModel>());
            AssertCurrentNode(current, "My workspace & reports");
            Assert.Equal("Dashboard", current.Name);
        }
        else
        {
            Assert.Equal("<h1 class=\"oc-breadcrumb-title\">My workspace &amp; reports</h1>", output.Content.GetContent());
            services.VerifyNoOtherCalls();
        }

        VerifyNoLinkServicesResolved(services);
    }

    [Fact]
    public void AddBreadcrumbProvider_RepeatedRegistration_PreservesOrderAndScopedLifetime()
    {
        var services = new ServiceCollection();
        services.AddBreadcrumbProvider<FirstBreadcrumbProvider>();
        services.AddBreadcrumbProvider<SecondBreadcrumbProvider>();
        services.AddBreadcrumbProvider<FirstBreadcrumbProvider>();
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        using var otherScope = serviceProvider.CreateScope();

        var providers = scope.ServiceProvider.GetServices<IBreadcrumbProvider>().ToArray();
        Assert.Collection(providers,
            provider => Assert.IsType<FirstBreadcrumbProvider>(provider),
            provider => Assert.IsType<SecondBreadcrumbProvider>(provider));
        Assert.Equal(providers, scope.ServiceProvider.GetServices<IBreadcrumbProvider>());
        var otherProviders = otherScope.ServiceProvider.GetServices<IBreadcrumbProvider>().ToArray();
        Assert.NotSame(providers[0], otherProviders[0]);
        Assert.NotSame(providers[1], otherProviders[1]);
    }

    [Fact]
    public async Task ProcessAsync_ItemWithoutParent_DoesNotEvaluateChildrenOrResolveServices()
    {
        var services = new Mock<IServiceProvider>(MockBehavior.Strict);
        var helper = new BreadcrumbItemTagHelper();
        var output = new TagHelperOutput("breadcrumb-item", [], (_, _) =>
            throw new InvalidOperationException("Children should not be evaluated."));

        await helper.ProcessAsync(CreateContext(), output);

        Assert.Empty(output.Content.GetContent());
        services.VerifyNoOtherCalls();
    }

    private static (BreadcrumbTagHelper Helper, PageTitleBuilder TitleBuilder, Mock<ISiteService> SiteService) CreateHelper(
        IServiceProvider services, bool showBreadcrumb = false, bool isAdmin = true)
    {
        var site = new SiteSettings();
        site.Put(new AdminSettings { ShowBreadcrumb = showBreadcrumb });
        var siteService = new Mock<ISiteService>(MockBehavior.Strict);
        siteService.Setup(value => value.GetSiteSettingsAsync()).ReturnsAsync(site);
        var titleBuilder = new PageTitleBuilder();
        var httpContext = new DefaultHttpContext { RequestServices = services };

        if (isAdmin)
        {
            AdminAttribute.Apply(httpContext);
        }

        var helper = new BreadcrumbTagHelper(titleBuilder, siteService.Object)
        {
            Name = "RecordsEdit",
            Title = new HtmlContentString("Title"),
            ViewContext = new ViewContext { HttpContext = httpContext },
        };

        return (helper, titleBuilder, siteService);
    }

    private static Task<TagHelperOutput> RenderAsync(
        BreadcrumbTagHelper helper, params (string Text, string Position)[] children)
        => RenderItemsAsync(helper, children.Select(child => (new BreadcrumbItemTagHelper
        {
            Id = "Record",
            Action = "Index",
            Controller = "Admin",
            Area = "My.Module",
            PermissionName = "ManageRecords",
            Position = child.Position,
        }, child.Text)).ToArray());

    private static async Task<TagHelperOutput> RenderItemsAsync(
        BreadcrumbTagHelper helper, params (BreadcrumbItemTagHelper Helper, string Text)[] children)
    {
        var context = CreateContext();
        var output = new TagHelperOutput("breadcrumb", [], async (_, _) =>
        {
            foreach (var (item, text) in children)
            {
                var childOutput = new TagHelperOutput("breadcrumb-item", [], (_, _) =>
                    Task.FromResult<TagHelperContent>(new DefaultTagHelperContent().SetHtmlContent(text)));

                await item.ProcessAsync(context, childOutput);
                Assert.Empty(childOutput.Content.GetContent());
            }

            return new DefaultTagHelperContent();
        });

        await helper.ProcessAsync(context, output);

        return output;
    }

    private static TagHelperContext CreateContext()
        => new([], new Dictionary<object, object>(), Guid.NewGuid().ToString());

    private static string GetTitle(PageTitleBuilder builder)
    {
        using var writer = new StringWriter();
        builder.GenerateTitle(null).WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }

    private static Mock<IServiceProvider> CreateServices(params IBreadcrumbProvider[] providers)
    {
        var services = new Mock<IServiceProvider>(MockBehavior.Strict);
        services.Setup(value => value.GetService(typeof(IEnumerable<IBreadcrumbProvider>))).Returns(providers);

        return services;
    }

    private static void VerifyOnlyProvidersResolved(Mock<IServiceProvider> services)
    {
        services.Verify(value => value.GetService(typeof(IEnumerable<IBreadcrumbProvider>)), Times.Once);
        services.VerifyNoOtherCalls();
    }

    private static void VerifyNoLinkServicesResolved(Mock<IServiceProvider> services)
    {
        services.Verify(value => value.GetService(typeof(IPermissionService)), Times.Never);
        services.Verify(value => value.GetService(typeof(IAuthorizationService)), Times.Never);
        services.Verify(value => value.GetService(typeof(IUrlHelperFactory)), Times.Never);
    }

    private static void AssertCurrentNode(BreadcrumbItemViewModel node, string title)
    {
        Assert.Equal(title, node.Text);
        Assert.Equal(title, node.Item.Text);
        Assert.Equal("Title", node.Item.Id);
        Assert.True(node.IsCurrent);
        Assert.True(node.Item.IsCurrent);
        Assert.Null(node.Href);
        Assert.Null(node.Item.Href);
        Assert.Null(node.Item.Url);
        Assert.Null(node.Item.RouteValues);
        Assert.Null(node.Item.PermissionName);
        Assert.Null(node.Item.Resource);
        Assert.Empty(node.Item.Permissions);
    }

    private static void AssertNormalizedTitle(
        TagHelperOutput output, CapturingDisplayHelper display, PageTitleBuilder titleBuilder,
        bool showBreadcrumb, string title, string encodedTitle)
    {
        Assert.Equal(encodedTitle, GetTitle(titleBuilder));
        if (showBreadcrumb)
        {
            Assert.Equal(title, display.Shape.GetProperty<string>("Title"));
            Assert.Equal("h1", display.Shape.GetProperty<string>("Heading"));
            AssertCurrentNode(display.Shape.Items.Cast<BreadcrumbItemViewModel>().Last(), title);
            Assert.Equal("rendered trail", output.Content.GetContent());
        }
        else
        {
            Assert.Null(display.Shape);
            Assert.Equal(string.IsNullOrEmpty(title) ? "" : $"<h1 class=\"oc-breadcrumb-title\">{encodedTitle}</h1>",
                output.Content.GetContent());
        }
    }

    private static DashboardBreadcrumbProvider CreateDashboardProvider()
    {
        var localizer = new Mock<IStringLocalizer<DashboardBreadcrumbProvider>>(MockBehavior.Strict);
        localizer.Setup(value => value["Dashboard"]).Returns(new LocalizedString("Dashboard", "Dashboard"));

        return new DashboardBreadcrumbProvider(
            Options.Create(new AdminOptions { AdminUrlPrefix = "Backend" }),
            localizer.Object);
    }

    private static CapturingDisplayHelper ConfigureRendering(Mock<IServiceProvider> services)
    {
        services.Setup(value => value.GetService(typeof(IShapeFactory))).Returns(new TestShapeFactory());
        var display = new CapturingDisplayHelper();
        services.Setup(value => value.GetService(typeof(IDisplayHelper))).Returns(display);

        return display;
    }

    private sealed class DelegateBreadcrumbProvider : IBreadcrumbProvider
    {
        private readonly Func<BreadcrumbContext, ValueTask> _callback;

        public DelegateBreadcrumbProvider(Func<BreadcrumbContext, ValueTask> callback)
        {
            _callback = callback;
        }

        public ValueTask BuildBreadcrumbAsync(BreadcrumbContext context) => _callback(context);
    }

    private sealed class FirstBreadcrumbProvider : IBreadcrumbProvider
    {
        public ValueTask BuildBreadcrumbAsync(BreadcrumbContext context) => ValueTask.CompletedTask;
    }

    private sealed class SecondBreadcrumbProvider : IBreadcrumbProvider
    {
        public ValueTask BuildBreadcrumbAsync(BreadcrumbContext context) => ValueTask.CompletedTask;
    }

    private sealed class CapturingDisplayHelper : IDisplayHelper
    {
        public IShape Shape { get; private set; }

        public Task<IHtmlContent> ShapeExecuteAsync(IShape shape)
        {
            Shape = shape;

            return Task.FromResult<IHtmlContent>(new HtmlString("rendered trail"));
        }
    }

    private sealed class TestShapeFactory : IShapeFactory
    {
        public dynamic New => throw new NotSupportedException();

        public async ValueTask<IShape> CreateAsync(
            string shapeType, Func<ValueTask<IShape>> shapeFactory,
            Action<ShapeCreatingContext> creating, Action<ShapeCreatedContext> created)
        {
            var shape = await shapeFactory();
            shape.Metadata.Type = shapeType;
            created?.Invoke(new ShapeCreatedContext { Shape = shape });

            return shape;
        }
    }
}
