using Fluid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.Forms.Activities;
using OrchardCore.Forms.Activities.Drivers;
using OrchardCore.Forms.Drivers;
using OrchardCore.Forms.Filters;
using OrchardCore.Forms.Models;
using OrchardCore.Modules;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Forms;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<FormPart>();
            o.MemberAccessStrategy.Register<FormElementPart>();
            o.MemberAccessStrategy.Register<FormInputElementPart>();
            o.MemberAccessStrategy.Register<LabelPart>();
            o.MemberAccessStrategy.Register<InputPart>();
            o.MemberAccessStrategy.Register<SelectPart>();
            o.MemberAccessStrategy.Register<TextAreaPart>();
            o.MemberAccessStrategy.Register<ButtonPart>();
        });

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ExportModelStateAttribute>();
            options.Filters.Add<ImportModelStateAttribute>();
            options.Filters.Add<ImportModelStatePageFilter>();
        });

        services.AddScoped<IContentDisplayDriver, FormContentDisplayDriver>();

        services.AddContentPart<FormPart>()
                .UseDisplayDriver<FormPartDisplayDriver>();

        services.AddContentPart<FormElementPart>()
                .UseDisplayDriver<FormElementPartDisplayDriver>();

        services.AddContentPart<FormInputElementPart>()
                .UseDisplayDriver<FormInputElementPartDisplayDriver>();

        services.AddContentPart<LabelPart>()
                .UseDisplayDriver<LabelPartDisplayDriver>();

        services.AddContentPart<ButtonPart>()
                .UseDisplayDriver<ButtonPartDisplayDriver>();

        services.AddContentPart<InputPart>()
                .UseDisplayDriver<InputPartDisplayDriver>();

        services.AddContentPart<SelectPart>()
            .UseDisplayDriver<SelectPartDisplayDriver>();

        services.AddContentPart<TextAreaPart>()
                .UseDisplayDriver<TextAreaPartDisplayDriver>();

        services.AddContentPart<ValidationSummaryPart>()
                .UseDisplayDriver<ValidationSummaryPartDisplayDriver>();

        services.AddContentPart<ValidationPart>()
                .UseDisplayDriver<ValidationPartDisplayDriver>();

        services.AddContentPart<FormElementLabelPart>()
                .UseDisplayDriver<FormElementLabelPartDisplayDriver>();

        services.AddContentPart<FormElementValidationPart>()
                .UseDisplayDriver<FormElementValidationPartDisplayDriver>();

        services.AddDataMigration<Migrations>();
        services.AddShapeTableProvider<FormShapeTableProvider>();
    }
}

[RequireFeatures("OrchardCore.Workflows")]
public sealed class WorkflowStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<HttpRedirectToFormLocationTask, HttpRedirectToFormLocationTaskDisplayDriver>();
    }
}
