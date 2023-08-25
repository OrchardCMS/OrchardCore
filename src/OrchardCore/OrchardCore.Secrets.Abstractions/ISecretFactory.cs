namespace OrchardCore.Secrets;

public interface ISecretFactory
{
    string Name { get; }
    Secret Create();
}
