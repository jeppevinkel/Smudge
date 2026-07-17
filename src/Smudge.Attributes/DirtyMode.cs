namespace Smudge;

/// <summary>
/// Determines if dirty is per property or aggregated
/// </summary>
public enum DirtyMode
{
    /// <summary>Dirty state is aggregated across all properties.</summary>
    Aggregated = 0,

    /// <summary>Each property tracks its own dirty state.</summary>
    PerProperty = 1,
}