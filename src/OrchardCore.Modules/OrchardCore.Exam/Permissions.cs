using OrchardCore.Security.Permissions;

namespace OrchardCore.Exam;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageExams = new("ManageExams", "Manage Exams");
    public static readonly Permission TakeExam = new("TakeExam", "Take Exams", new[] { ManageExams });
    public static readonly Permission ManageQuestionBank = new("ManageQuestionBank", "Manage Question Bank", new[] { ManageExams });
    public static readonly Permission ManageExamPapers = new("ManageExamPapers", "Manage Exam Papers", new[] { ManageExams });
    public static readonly Permission ManageExamAssignments = new("ManageExamAssignments", "Manage Exam Assignments", new[] { ManageExams });
    public static readonly Permission GradeExams = new("GradeExams", "Grade Exams", new[] { ManageExams });
    public static readonly Permission ViewExamRecords = new("ViewExamRecords", "View Exam Records", new[] { ManageExams });

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult<IEnumerable<Permission>>(new[]
        {
            ManageExams,
            TakeExam,
            ManageQuestionBank,
            ManageExamPapers,
            ManageExamAssignments,
            GradeExams,
            ViewExamRecords
        });
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[]
        {
            new PermissionStereotype
            {
                Stereotype = "Administrator",
                Permissions = new[] { ManageExams, TakeExam, ManageQuestionBank, ManageExamPapers, ManageExamAssignments, GradeExams, ViewExamRecords }
            },
            new PermissionStereotype
            {
                Stereotype = "Authenticated",
                Permissions = new[] { TakeExam }
            }
        };
    }
}
