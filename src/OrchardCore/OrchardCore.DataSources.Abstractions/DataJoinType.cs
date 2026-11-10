namespace OrchardCore.DataSources;

/// <summary>
/// Identifies which rows without a match a join keeps.
/// </summary>
public enum DataJoinType
{
    /// <summary>
    /// Only the rows that match.
    /// </summary>
    Inner,

    /// <summary>
    /// Every row of the left input; the fields of the right input are empty when there is no match.
    /// </summary>
    Left,

    /// <summary>
    /// Every row of the right input; the fields of the left input are empty when there is no match.
    /// </summary>
    Right,

    /// <summary>
    /// Every row of both inputs.
    /// </summary>
    Full,
}
