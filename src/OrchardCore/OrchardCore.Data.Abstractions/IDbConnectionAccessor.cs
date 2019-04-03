using System.Data.Common;

namespace OrchardCore.Data
{
    public interface IDbConnectionAccessor
    {
        DbConnection CreateConnection();
    }
}
