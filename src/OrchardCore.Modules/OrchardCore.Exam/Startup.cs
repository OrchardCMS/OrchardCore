using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Exam.Drivers;
using OrchardCore.Exam.Models;
using OrchardCore.Exam.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Exam;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Data migration
        services.AddDataMigration<Migrations>();

        // Permissions
        services.AddPermissionProvider<Permissions>();

        // Navigation
        services.AddNavigationProvider<AdminMenu>();

        // Content parts and drivers
        services.AddContentPart<QuestionPart>()
            .UseDisplayDriver<QuestionPartDisplayDriver>();

        services.AddContentPart<ExamPaperPart>()
            .UseDisplayDriver<ExamPaperPartDisplayDriver>();

        services.AddContentPart<ExamAssignmentPart>()
            .UseDisplayDriver<ExamAssignmentPartDisplayDriver>();

        services.AddContentPart<ExamRecordPart>()
            .UseDisplayDriver<ExamRecordPartDisplayDriver>();

        // Services
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IExamPaperService, ExamPaperService>();
        services.AddScoped<IExamService, ExamService>();

        // Fluid template access
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<QuestionPart>();
            o.MemberAccessStrategy.Register<ExamPaperPart>();
            o.MemberAccessStrategy.Register<ExamAssignmentPart>();
            o.MemberAccessStrategy.Register<ExamRecordPart>();
            o.MemberAccessStrategy.Register<ExamSection>();
            o.MemberAccessStrategy.Register<ExamSectionRules>();
            o.MemberAccessStrategy.Register<AnswerEntry>();
        });
    }
}
