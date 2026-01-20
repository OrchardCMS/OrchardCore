using System.Linq;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.ViewModels
{
    public class GroupViewModel : Shape
    {
        public string Identifier { get; set; }
    }

    public class GroupingsViewModel : GroupViewModel
    {
        public IGrouping<string, object>[] Groupings { get; set; }
    }

    public class GroupingViewModel : GroupViewModel
    {
        public IGrouping<string, object> Grouping { get; set; }
    }
}
