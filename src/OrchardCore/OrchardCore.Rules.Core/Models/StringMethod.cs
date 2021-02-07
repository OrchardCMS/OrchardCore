using System;

namespace OrchardCore.Rules.Models
{
    public class StringMethod : Rule
    {
        public string Value { get; set; }
        public Operator Operator { get; set; }
    }    
}