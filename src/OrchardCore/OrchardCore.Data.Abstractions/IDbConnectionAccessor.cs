using System;
using System.Data;
using System.Threading.Tasks;

namespace OrchardCore.Data.Abstractions
{
    public interface IDbConnectionAccessor
    {
        Task<IDbConnection> GetConnectionAsync();
    }
}
