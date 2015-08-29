namespace Orchard.Hosting.Console.HostContext {
    public interface ICommandHostContextProvider {
        CommandHostContext CreateContext();
        void Shutdown(CommandHostContext context);
    }
}
