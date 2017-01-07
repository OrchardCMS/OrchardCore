namespace Orchard.Glimpse.Inspectors.Ado.Proxies
{
    public interface IDbCommand
    {
        string CommandText { get; }

        object CommandType { get; }

        object Parameters { get; }
    }
}
