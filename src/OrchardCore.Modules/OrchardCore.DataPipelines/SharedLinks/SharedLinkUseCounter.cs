using Dapper;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Counts the uses of the shared links in a table of their own, <c>SharedLinkUses</c>, out of the documents of the links
/// and of their index, which YesSql rewrites on every save. A use is granted by one conditional update, which the
/// database applies atomically: two requests can't both take the last use of a link.
/// </summary>
public sealed class SharedLinkUseCounter
{
    /// <summary>
    /// The name of the table, without the table prefix of the tenant.
    /// </summary>
    public const string TableName = "SharedLinkUses";

    private readonly ISession _session;

    public SharedLinkUseCounter(ISession session)
    {
        _session = session;
    }

    /// <summary>
    /// Adds the counter of a new link, at zero uses, in the transaction of the session.
    /// </summary>
    /// <param name="linkId">The identifier of the link.</param>
    /// <returns>A task that completes when the counter is added.</returns>
    public async Task AddAsync(string linkId)
    {
        var (transaction, table, dialect) = await GetCommandContextAsync();

        await transaction.Connection.ExecuteAsync(
            $"insert into {table} ({dialect.QuoteForColumnName("LinkId")}, {dialect.QuoteForColumnName("Uses")}) values (@LinkId, 0);",
            new { LinkId = linkId },
            transaction);
    }

    /// <summary>
    /// Takes a use of a link, unless it was used as many times as allowed.
    /// </summary>
    /// <param name="linkId">The identifier of the link.</param>
    /// <param name="maxUses">How many times the link can be used, or <c>0</c> for no limit.</param>
    /// <returns>Whether the use was granted, and the number of uses of the link.</returns>
    public async Task<(bool Granted, int Uses)> TryUseAsync(string linkId, int maxUses)
    {
        if (await TryIncrementAsync(linkId, maxUses) == 0)
        {
            // A link saved without its counter, such as by an import, gets one.
            if (await CountRowsAsync(linkId) > 0)
            {
                return (false, await GetUsesAsync(linkId));
            }

            await AddAsync(linkId);

            if (await TryIncrementAsync(linkId, maxUses) == 0)
            {
                return (false, 0);
            }
        }

        return (true, await GetUsesAsync(linkId));
    }

    /// <summary>
    /// Returns the number of uses of links.
    /// </summary>
    /// <param name="linkIds">The identifiers of the links.</param>
    /// <returns>The number of uses, by link. A link without a counter is not in it.</returns>
    public async Task<IReadOnlyDictionary<string, int>> GetUsesAsync(IReadOnlyCollection<string> linkIds)
    {
        if (linkIds.Count == 0)
        {
            return new Dictionary<string, int>(StringComparer.Ordinal);
        }

        var (transaction, table, dialect) = await GetCommandContextAsync();
        var rows = await transaction.Connection.QueryAsync<(string LinkId, int Uses)>(
            $"select {dialect.QuoteForColumnName("LinkId")}, {dialect.QuoteForColumnName("Uses")} from {table} where {dialect.QuoteForColumnName("LinkId")} {dialect.InOperator("@Ids")};",
            new { Ids = linkIds },
            transaction);

        return rows.ToDictionary(row => row.LinkId, row => row.Uses, StringComparer.Ordinal);
    }

    /// <summary>
    /// Deletes the counter of a link, in the transaction of the session.
    /// </summary>
    /// <param name="linkId">The identifier of the link.</param>
    /// <returns>A task that completes when the counter is deleted.</returns>
    public async Task DeleteAsync(string linkId)
    {
        var (transaction, table, dialect) = await GetCommandContextAsync();

        await transaction.Connection.ExecuteAsync(
            $"delete from {table} where {dialect.QuoteForColumnName("LinkId")} = @LinkId;",
            new { LinkId = linkId },
            transaction);
    }

    // The conditional update that grants a use: it changes no row once the link is used up.
    private async Task<int> TryIncrementAsync(string linkId, int maxUses)
    {
        var (transaction, table, dialect) = await GetCommandContextAsync();
        var uses = dialect.QuoteForColumnName("Uses");
        var limit = maxUses > 0 ? $" and {uses} < @MaxUses" : string.Empty;

        return await transaction.Connection.ExecuteAsync(
            $"update {table} set {uses} = {uses} + 1 where {dialect.QuoteForColumnName("LinkId")} = @LinkId{limit};",
            new { LinkId = linkId, MaxUses = maxUses },
            transaction);
    }

    private async Task<int> CountRowsAsync(string linkId)
    {
        var (transaction, table, dialect) = await GetCommandContextAsync();

        return await transaction.Connection.ExecuteScalarAsync<int>(
            $"select count(*) from {table} where {dialect.QuoteForColumnName("LinkId")} = @LinkId;",
            new { LinkId = linkId },
            transaction);
    }

    private async Task<int> GetUsesAsync(string linkId)
    {
        return (await GetUsesAsync([linkId])).GetValueOrDefault(linkId);
    }

    private async Task<(System.Data.Common.DbTransaction Transaction, string Table, ISqlDialect Dialect)> GetCommandContextAsync()
    {
        var transaction = await _session.BeginTransactionAsync();
        var configuration = _session.Store.Configuration;
        var table = configuration.SqlDialect.QuoteForTableName(configuration.TablePrefix + TableName, configuration.Schema);

        return (transaction, table, configuration.SqlDialect);
    }
}
