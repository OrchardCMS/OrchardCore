using YesSql.Filters.Query;

namespace OrchardCore.ContentsTransfer.Services;

public class DefaultContentTypeEntryAdminListFilterParser : IContentTransferEntryAdminListFilterParser
{
    private readonly IQueryParser<ContentTransferEntry> _parser;

    public DefaultContentTypeEntryAdminListFilterParser(IQueryParser<ContentTransferEntry> parser)
    {
        _parser = parser;
    }

    public QueryFilterResult<ContentTransferEntry> Parse(string text)
        => _parser.Parse(text);
}
