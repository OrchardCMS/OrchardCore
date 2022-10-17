using YesSql.Filters.Query;

namespace OrchardCore.Notifications.Models;


public class DefaultNotificationAdminListFilterParser : INotificationAdminListFilterParser
{
    private readonly IQueryParser<WebNotification> _parser;

    public DefaultNotificationAdminListFilterParser(IQueryParser<WebNotification> parser)
    {
        _parser = parser;
    }

    public QueryFilterResult<WebNotification> Parse(string text)
        => _parser.Parse(text);
}
