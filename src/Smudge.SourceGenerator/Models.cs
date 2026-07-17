using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Smudge.SourceGenerator;

internal sealed record ClassModel(
    string Name,
    string? Namespace,
    string HintName,
    bool ShouldGenerate,
    DirtyMode Mode,
    EquatableArray<PropertyModel> Properties,
    EquatableArray<DiagnosticInfo> Diagnostics);

internal sealed record PropertyModel(
    string PropertyName,
    string FieldName,
    string TypeName,
    string Accessibility,
    string DefaultValue);

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    LocationInfo? Location,
    EquatableArray<string> MessageArgs)
{
    public Diagnostic ToDiagnostic() => Diagnostic.Create(
        Descriptor,
        Location?.ToLocation() ?? Microsoft.CodeAnalysis.Location.None,
        MessageArgs.Items.Cast<object>().ToArray());
}

internal sealed record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

    public static LocationInfo? From(Location? location) =>
        location?.SourceTree is null
            ? null
            : new LocationInfo(location.SourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
}