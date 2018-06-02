using OrchardCore.Localization.Models;
using YesSql.Indexes;

namespace OrchardCore.Localization.Indexes
{
    public class CultureIndex : MapIndex
    {
        public string Culture { get; set; }
    }

    public class CultureIndexProvider : IndexProvider<CultureRecord>
    {
        public override void Describe(DescribeContext<CultureRecord> context)
        {
            context.For<CultureIndex>()
                .Map(culture =>
                {
                    return new CultureIndex
                    {
                        Culture = culture.Culture
                    };
                });
        }
    }
}