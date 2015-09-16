namespace Orchard.Configuration.Environment {
    public static class ShellHelper
    {
        public const string DefaultShellName = "Default";

        public static ShellSettings BuildDefaultUninitializedShell = new ShellSettings {
            Name = DefaultShellName,
            State = TenantState.Uninitialized
        };
    }
}
