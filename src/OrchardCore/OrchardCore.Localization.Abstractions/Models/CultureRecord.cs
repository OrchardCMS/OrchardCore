using System.Collections.Generic;
using OrchardCore.Entities;

namespace OrchardCore.Localization.Models
{
    public class CultureRecord : Entity
    {
        public CultureRecord()
        {
            Cultures = new List<Culture>();
        }
        public int Id { get; set; }
        
        public List<Culture> Cultures { get; set; }
    }

    public class Culture
    {
        public string CultureName { get; set; }

        public override string ToString()
        {
            return CultureName;
        }
    }
}
