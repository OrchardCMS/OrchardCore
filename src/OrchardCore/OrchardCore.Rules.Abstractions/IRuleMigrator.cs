namespace OrchardCore.Rules
{
    /// <comment>
    /// Migrates existing script rules to rules.
    /// </comment>
    /// <remarks>
    /// For migrations purposes only. This code can be removed in a later release.
    /// </remarks>
    public interface IRuleMigrator
    {
        void Migrate(string existingRule, Rule rule);
    }
}
