using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Data
{
    public interface IDbConnectionWrapper
    {
        Task<IDbConnection> GetConnectionAsync();
    }
}
