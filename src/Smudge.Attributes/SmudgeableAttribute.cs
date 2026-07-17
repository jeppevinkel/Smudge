using System;

namespace Smudge;

/// <summary>
/// Mark a class as smudgeable.
/// Smudgeable classes have all partial properties tracked for changes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SmudgeableAttribute(DirtyMode dirtyMode = DirtyMode.Aggregated) : Attribute
{
    /// <summary>
    /// Specifies the mode used to track changes in a smudgeable object.
    /// </summary>
    /// <remarks>
    /// The <see cref="DirtyMode"/> determines whether changes are tracked on a per-property basis
    /// (<see cref="DirtyMode.PerProperty"/>) or aggregated across all properties
    /// (<see cref="DirtyMode.Aggregated"/>).
    /// </remarks>
    public DirtyMode DirtyMode { get; } = dirtyMode;
}