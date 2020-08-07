using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public class AesSecret : Secret
    {
        public string Key1 { get; set; }
        public string Key2 { get; set; }
    }
}
