namespace Smudge;

/// <summary>
/// A type whose changes are tracked. Implemented by the Smudge source generator
/// for classes marked with <see cref="SmudgeableAttribute"/>.
/// </summary>
public interface ISmudgeable
{
    /// <summary>Whether any tracked property has changed since the last <see cref="WipeClean"/>.</summary>
    bool IsDirty { get; }

    /// <summary>Resets all dirty state.</summary>
    void WipeClean();
}