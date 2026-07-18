using System;
using System.Collections.Generic;

namespace Smudge;

/// <summary>
/// A type whose changes are tracked. Implemented by the Smudge source generator
/// for classes marked with <see cref="SmudgeableAttribute"/> in PerProperty mode.
/// </summary>
public interface IPerPropertySmudgeable : ISmudgeable
{
    /// <summary>
    /// Returns true if the property value has been modified since the last <see cref="ISmudgeable.WipeClean"/>.
    /// </summary>
    /// <param name="propertyName">The name of the property to check.</param>
    /// <returns><see langword="true"/> if the property has changed; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> does not match any tracked property.</exception>
    bool IsPropertyDirty(string propertyName);
    
    /// <summary>
    /// Returns a collection of property names that have been modified since the last time they were wiped clean.
    /// </summary>
    IReadOnlyCollection<string> DirtyProperties { get; }
}