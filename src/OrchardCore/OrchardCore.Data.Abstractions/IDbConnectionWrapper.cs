using System;
using System.Data;
using System.Threading.Tasks;

namespace OrchardCore.Data
{
    public interface IDbConnectionWrapper
    {
        Task<IDbConnection> GetConnectionAsync();
    }
}
