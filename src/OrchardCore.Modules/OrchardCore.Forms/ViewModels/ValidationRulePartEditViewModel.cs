using System.Collections.Generic;

namespace OrchardCore.Forms.ViewModels
{
    public class ValidationRulePartEditViewModel
    {
        public string Type { get; set; }
        public string Option { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<ValidationOptionViewModel> ValidationOptionViewModels { get;set;  }
    }
    public class ValidationOptionViewModel
    {
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public bool IsShowOption { get; set; }
        public string OptionPlaceHolder { get; set; }
        public bool IsShowErrorMessage { get; set; }
    }
}
