using OrchardCore.Entities;

namespace OrchardCore.Localization.Models
{
    public class CultureRecord : Entity
    {
        public int Id { get; set; }
        public string Culture { get; set; }

        public override string ToString()
        {            
            return Culture;
        }
    }
}
