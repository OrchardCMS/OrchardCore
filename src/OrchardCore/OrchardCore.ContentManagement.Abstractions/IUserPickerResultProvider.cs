using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement
{
    public interface IUserPickerResultProvider
    {
        string Name { get; }
        Task<IEnumerable<UserPickerResult>> Search(UserPickerSearchContext searchContext);
    }

    public class UserPickerSearchContext
    {
        public string Query { get; set; }
        public bool DisplayAllUsers { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }

    public class UserPickerResult
    {
        public string DisplayText { get; set; }
        public string UserId { get; set; }
        public bool IsEnabled { get; set; }
    }
}
