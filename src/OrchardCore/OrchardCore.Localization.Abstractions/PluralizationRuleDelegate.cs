namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents the method that will get the position of the pluralization form.
    /// </summary>
    /// <param name="count">The number used to specify the pluralization form.</param>
    /// <returns>The pluralization rule position.</returns>
    public delegate int PluralizationRuleDelegate(int count);
}
