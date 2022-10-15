using System;
using Microsoft.Extensions.Options;
using OrchardCore.Data.YesSql;
using YesSql.Services;

namespace OrchardCore.Data
{
    public class YesSqlOptionsConfiguration : IConfigureNamedOptions<YesSqlOptions>
    {
        public void Configure(YesSqlOptions options) => Configure(String.Empty, options);

        public void Configure(string name, YesSqlOptions options)
        {
            options.TableNameConvention ??= new DefaultTableNameConvention();
        }
    }
}
