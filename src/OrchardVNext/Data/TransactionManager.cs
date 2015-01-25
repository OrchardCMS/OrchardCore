namespace OrchardVNext.Data
{
    public interface ITransactionManager : IDependency {
        void Demand();
        void RequireNew();
        void Cancel();
    }
}