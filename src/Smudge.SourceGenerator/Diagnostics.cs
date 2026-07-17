using System.Linq;
using Microsoft.CodeAnalysis;

namespace Smudge.SourceGenerator;

internal static class Diagnostics
{
    internal static readonly DiagnosticDescriptor WrongArgumentCount = new(
        "SMDG001", "Invalid [SmudgeDefault] usage",
        "[SmudgeDefault] on non-collection property '{0}' must have exactly one argument (found {1})",
        "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    
    internal static readonly DiagnosticDescriptor TypeMismatch = new(
        "SMDG002", "Type mismatch in [SmudgeDefault]",
        "Value '{0}' of type '{1}' does not match type '{2}' of property '{3}'",
        "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    
    internal static readonly DiagnosticDescriptor ClassMustBePartial = new(
        "SMDG003", "[Smudgeable] class must be partial",
        "Class '{0}' is marked [Smudgeable] but is not declared partial",
        "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    
    internal static readonly DiagnosticDescriptor UnsupportedClassShape = new(
        "SMDG004", "Unsupported [Smudgeable] class",
        "Class '{0}' cannot be generic or nested when marked [Smudgeable]",
        "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    
    internal static readonly DiagnosticDescriptor PropertyMustHaveGetAndSet = new(
        "SMDG005", "Partial property must declare get and set",
        "Partial property '{0}' must declare both a getter and a setter to be dirty-tracked",
        "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    
    internal static readonly DiagnosticDescriptor InvalidDirtyMode = new(
        "SMDG006", "Invalid DirtyMode value",
        "'{0}' is not a valid DirtyMode value",
        "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor TooManyProperties = new(
        "SMDG007", "Too many tracked properties",
        "Class '{0}' has {1} tracked properties; PerProperty mode supports at most 64",
        "Usage", DiagnosticSeverity.Error, isEnabledByDefault: true);
    
    internal static DiagnosticInfo Diag(DiagnosticDescriptor descriptor, LocationInfo? location, params object?[] args) =>
        new(descriptor, location, new EquatableArray<string>(
            args.Select(a => a?.ToString() ?? "null").ToArray()));
}