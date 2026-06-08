using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Exam.Models;

namespace OrchardCore.Exam;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        // Question content type
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Question", type => type
            .Draftable()
            .Versionable()
            .Creatable()
            .Listable()
            .WithPart("TitlePart", part => part.WithPosition("0"))
            .WithPart("QuestionPart", part => part.WithPosition("1"))
        );

        // QuestionPart definition
        await _contentDefinitionManager.AlterPartDefinitionAsync("QuestionPart", part => part
            .WithField("Category", field => field
                .OfType("TaxonomyField")
                .WithDisplayName("Category")
                .WithSettings(new TaxonomyFieldSettings
                {
                    TaxonomyContentItemId = string.Empty,
                    Unique = true,
                })
            )
        );

        // ExamPaper content type
        await _contentDefinitionManager.AlterTypeDefinitionAsync("ExamPaper", type => type
            .Draftable()
            .Versionable()
            .Creatable()
            .Listable()
            .WithPart("TitlePart", part => part.WithPosition("0"))
            .WithPart("ExamPaperPart", part => part.WithPosition("1"))
        );

        // ExamAssignment content type
        await _contentDefinitionManager.AlterTypeDefinitionAsync("ExamAssignment", type => type
            .Draftable()
            .Versionable()
            .Creatable()
            .Listable()
            .WithPart("TitlePart", part => part.WithPosition("0"))
            .WithPart("ExamAssignmentPart", part => part.WithPosition("1"))
        );

        // ExamRecord content type - NOT creatable by users
        await _contentDefinitionManager.AlterTypeDefinitionAsync("ExamRecord", type => type
            .Draftable()
            .Versionable()
            .Listable()
            .WithPart("TitlePart", part => part.WithPosition("0"))
            .WithPart("ExamRecordPart", part => part.WithPosition("1"))
        );

        return 1;
    }
}

// Internal settings class to avoid dependency on Autoroute module
internal sealed class TaxonomyFieldSettings
{
    public string TaxonomyContentItemId { get; set; } = string.Empty;
    public bool Unique { get; set; }
}
