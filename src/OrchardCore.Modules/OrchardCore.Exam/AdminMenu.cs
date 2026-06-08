using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Exam;

namespace OrchardCore.Exam;

public class AdminMenu : INavigationProvider
{
    private readonly IStringLocalizer<AdminMenu> T;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        T = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        builder.Add(T["Exam"], "exam", exam => exam
            .Add(T["Questions"], "q", q => q
                .Action("List", "Admin", new { area = "OrchardCore.Contents", contentTypeId = "Question" })
                .Permission(Permissions.ManageQuestionBank)
                .LocalNav())
            .Add(T["Exam Papers"], "p", p => p
                .Action("List", "Admin", new { area = "OrchardCore.Contents", contentTypeId = "ExamPaper" })
                .Permission(Permissions.ManageExamPapers)
                .LocalNav())
            .Add(T["Exam Assignments"], "a", a => a
                .Action("List", "Admin", new { area = "OrchardCore.Contents", contentTypeId = "ExamAssignment" })
                .Permission(Permissions.ManageExamAssignments)
                .LocalNav())
            .Add(T["Exam Records"], "r", r => r
                .Action("List", "Admin", new { area = "OrchardCore.Contents", contentTypeId = "ExamRecord" })
                .Permission(Permissions.ViewExamRecords)
                .LocalNav())
        );

        return Task.CompletedTask;
    }
}
