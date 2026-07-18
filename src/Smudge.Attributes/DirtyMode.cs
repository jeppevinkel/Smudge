namespace Smudge;

/// <summary>
/// Specifies how dirty state is tracked in a smudgeable class.
/// </summary>
public enum DirtyMode
{
    /// <summary>Dirty state is aggregated across all properties.</summary>
    Aggregated = 0,

    /// <summary>Each property tracks its own dirty state.</summary>
    PerProperty = 1,
}