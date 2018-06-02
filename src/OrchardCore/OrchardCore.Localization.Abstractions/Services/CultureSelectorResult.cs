using System;
using System.Threading.Tasks;

namespace OrchardCore.Localization.Services
{
    public class CultureSelectorResult
    {
        public int Priority { get; set; }
        public Func<Task<string>> Name { get; set; }
    }
}
