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
    /// Returns true if the property value has been modified since the last time it was wiped clean.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Throws if the property name doesn't match any defined property.</exception>
    bool IsPropertyDirty(string propertyName);
    
    /// <summary>
    /// Returns a collection of property names that have been modified since the last time they were wiped clean.
    /// </summary>
    IReadOnlyCollection<string> DirtyProperties { get; }
}