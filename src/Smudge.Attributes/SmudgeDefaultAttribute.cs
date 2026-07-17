using System;

namespace Smudge;

/// <summary>
/// Defines the default value of tracked properties in a smudgeable class.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SmudgeDefaultAttribute(params object?[] values) : Attribute
{
    /// <summary>
    /// The default value(s) assigned to a tracked property. A single value for scalar
    /// properties; one or more elements for collection properties.
    /// </summary>
    public object?[] Values { get; } = values;
}