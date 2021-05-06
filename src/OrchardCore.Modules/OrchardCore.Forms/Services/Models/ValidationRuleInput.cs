using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Forms.Services.Models
{
    public class ValidationRuleInput
    {
        public string Input { get; set; }
        public string Type { get; set; }
        public string Option { get; set; }
        public string ErrorMesage { get; set; }
    }
}
