using YesSql.Filters.Query;

namespace OrchardCore.Notifications.Services;

public class DefaultNotificationAdminListFilterParser : INotificationAdminListFilterParser
{
    private readonly IQueryParser<Notification> _parser;

    public DefaultNotificationAdminListFilterParser(IQueryParser<Notification> parser)
    {
        _parser = parser;
    }

    public QueryFilterResult<Notification> Parse(string text)
        => _parser.Parse(text);
}
