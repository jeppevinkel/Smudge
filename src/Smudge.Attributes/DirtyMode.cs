namespace Smudge;

/// <summary>
/// Determines if dirty is per property or aggregated
/// </summary>
public enum DirtyMode
{
    /// <summary>
    /// Each property is dirty if it has been modified
    /// </summary>
    PerProperty,
    /// <summary>
    /// Dirty is aggregated across all properties
    /// </summary>
    Aggregated
}