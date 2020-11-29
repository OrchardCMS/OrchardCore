using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public class TextSecret : Secret
    {
        public string Text { get; set; }
    }
}
