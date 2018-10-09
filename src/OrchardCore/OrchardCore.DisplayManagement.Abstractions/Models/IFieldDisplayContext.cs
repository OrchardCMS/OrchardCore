
namespace OrchardCore.DisplayManagement.Models
{
    public interface IFieldDisplayManagementContext
    {
        // set to display within a specific part shape, instead of entire zone
        string ExplicitPartName { get; set; }
    }
}
