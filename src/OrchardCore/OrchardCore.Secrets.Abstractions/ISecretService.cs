using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface ISecretService<T> where T : Secret, new()
    {
          Task<T> GetSecretAsync(string key);
    }
}
