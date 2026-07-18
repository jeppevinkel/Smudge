using System;

namespace Smudge;

/// <summary>
/// Marks a class as smudgeable. The Smudge source generator will implement
/// ISmudgeable (or IPerPropertySmudgeable when using
/// <see cref="DirtyMode.PerProperty"/>) and track all <see langword="partial"/> properties for changes.
/// The class must be declared <see langword="partial"/>.
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