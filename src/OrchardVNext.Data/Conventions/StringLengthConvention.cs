using System.ComponentModel.DataAnnotations;

namespace OrchardVNext.Data.Conventions {
    public class StringLengthMaxAttribute : StringLengthAttribute {
        public StringLengthMaxAttribute() : base(10000) {
            // 10000 is an arbitrary number large enough to be in the nvarchar(max) range 
        }
    }
}