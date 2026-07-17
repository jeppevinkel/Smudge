using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
// ReSharper disable MergeIntoPattern

namespace Smudge.SourceGenerator;

internal static class Transformer
{
    private const string SmudgeDefaultAttributeName = "Smudge.SmudgeDefaultAttribute";
    
    private static readonly SymbolDisplayFormat FullyQualifiedNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat
            .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    internal static ClassModel Transform(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var classDecl = (ClassDeclarationSyntax)ctx.TargetNode;
        var diagnostics = new List<DiagnosticInfo>();

        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();
        var hintName = (ns is null ? symbol.Name : $"{ns}.{symbol.Name}") + ".g.cs";
        var identifierLocation = LocationInfo.From(classDecl.Identifier.GetLocation());

        var shouldGenerate = true;
        
        // --- DirtyMode: validate before trusting; (DirtyMode)5 is legal C# ---
        var mode = DirtyMode.Aggregated;
        var ctorArgs = ctx.Attributes[0].ConstructorArguments;
        if (ctorArgs.Length == 1 && ctorArgs[0].Value is int rawMode)
        {
            if (rawMode is (int)DirtyMode.Aggregated or (int)DirtyMode.PerProperty)
            {
                mode = (DirtyMode)rawMode;
            }
            else
            {
                diagnostics.Add(Diagnostics.Diag(Diagnostics.InvalidDirtyMode, identifierLocation, rawMode));
                shouldGenerate = false;
            }
        }
        
        // --- shape checks ---
        if (!classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            diagnostics.Add(Diagnostics.Diag(Diagnostics.ClassMustBePartial, identifierLocation, symbol.Name));
            shouldGenerate = false;
        }

        if (symbol.IsGenericType || symbol.ContainingType is not null)
        {
            diagnostics.Add(Diagnostics.Diag(Diagnostics.UnsupportedClassShape, identifierLocation, symbol.Name));
            shouldGenerate = false;
        }
        
        // --- properties ---
        var properties = new List<PropertyModel>();
        if (shouldGenerate)
        {
            foreach (var member in symbol.GetMembers())
            {
                ct.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol property) continue;
                if (!property.IsPartialDefinition || property.IsStatic || property.IsIndexer) continue;

                if (property.GetMethod is null || property.SetMethod is null)
                {
                    diagnostics.Add(Diagnostics.Diag(Diagnostics.PropertyMustHaveGetAndSet,
                        LocationInfo.From(property.Locations.FirstOrDefault()), property.Name));
                    continue;
                }

                properties.Add(new PropertyModel(
                    property.Name,
                    "_" + char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1),
                    property.Type.ToDisplayString(FullyQualifiedNullableFormat),
                    SyntaxFacts.GetText(property.DeclaredAccessibility),
                    ComputeDefaultValue(property, diagnostics, ct)));
            }
        }
        
        // --- bitmask cap: the emitter assumes index < 64 ---
        if (mode == DirtyMode.PerProperty && properties.Count > 64)
        {
            diagnostics.Add(Diagnostics.Diag(Diagnostics.TooManyProperties,
                identifierLocation, symbol.Name, properties.Count));
            shouldGenerate = false;
        }

        return new ClassModel(
            symbol.Name, ns, hintName, shouldGenerate, mode,
            new EquatableArray<PropertyModel>(properties.ToArray()),
            new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
    }

    private static string ComputeDefaultValue(
        IPropertySymbol property, List<DiagnosticInfo> diagnostics, CancellationToken ct)
    {
        var attribute = property
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == SmudgeDefaultAttributeName);

        if (attribute is not { ConstructorArguments.Length: 1 })
            return "default";

        var arg = attribute.ConstructorArguments[0];
        var location = LocationInfo.From(
            attribute.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation());
        
        // [SmudgeDefault(null)]: null binds to the params ARRAY itself.
        // arg.Values would throw here — this guard is load-bearing.
        if (arg.IsNull)
        {
            if (AcceptsNull(property.Type))
                return "default";

            diagnostics.Add(Diagnostics.Diag(Diagnostics.TypeMismatch, location,
                "null", "null", property.Type.ToDisplayString(), property.Name));
            return "default";
        }

        if (IsCollectionType(property.Type))
        {
            var elementType = GetCollectionElementType(property.Type);
            if (elementType is null)
                return "default";

            var mismatched = false;
            foreach (var value in arg.Values)
            {
                if (TypeMatches(elementType, value)) continue;
                mismatched = true;
                diagnostics.Add(Diagnostics.Diag(Diagnostics.TypeMismatch, location,
                    FormatConstant(value),
                    value.Type?.ToDisplayString() ?? "null",
                    elementType.ToDisplayString(),
                    property.Name));
            }

            return mismatched
                ? "default"
                : $"[{string.Join(", ", arg.Values.Select(FormatConstant))}]";
        }
        
        if (arg.Values.Length == 1)
        {
            var value = arg.Values[0];
            if (TypeMatches(property.Type, value))
                return FormatConstant(value);

            diagnostics.Add(Diagnostics.Diag(Diagnostics.TypeMismatch, location,
                FormatConstant(value),
                value.Type?.ToDisplayString() ?? "null",
                property.Type.ToDisplayString(),
                property.Name));
            return "default";
        }

        // Wrong count: report, but still emit a default-initialized property
        // so SMDG001 is the only error the user sees (no CS9248 cascade).
        diagnostics.Add(Diagnostics.Diag(Diagnostics.WrongArgumentCount,
            location, property.Name, arg.Values.Length));
        return "default";
    }
    
    // ------------------------------------------------------------------
    // Constant formatting & type compatibility
    // ------------------------------------------------------------------
    
    private static string FormatConstant(TypedConstant constant)
    {
        if (constant.IsNull) return "default";
        if (constant.Kind == TypedConstantKind.Array) return "default"; // nested arrays unsupported

        if (constant.Kind == TypedConstantKind.Enum)
            return $"({constant.Type!.ToDisplayString(FullyQualifiedNullableFormat)})({Convert.ToString(constant.Value, CultureInfo.InvariantCulture)})";

        if (constant.Kind == TypedConstantKind.Type)
            return $"typeof({((ITypeSymbol)constant.Value!).ToDisplayString(FullyQualifiedNullableFormat)})";

        return constant.Value switch
        {
            // FormatLiteral handles escaping ("a\"b", newlines, backslashes...)
            string s => SymbolDisplay.FormatLiteral(s, quote: true),
            char c   => SymbolDisplay.FormatLiteral(c, quote: true),
            bool b   => b ? "true" : "false",

            // Invariant culture (no "1,5" on de-DE build machines) + suffixes
            // so float/double literals actually compile against the field type.
            float f when float.IsNaN(f)                => "float.NaN",
            float f when float.IsPositiveInfinity(f)   => "float.PositiveInfinity",
            float f when float.IsNegativeInfinity(f)   => "float.NegativeInfinity",
            float f                                    => f.ToString("R", CultureInfo.InvariantCulture) + "f",
            double d when double.IsNaN(d)              => "double.NaN",
            double d when double.IsPositiveInfinity(d) => "double.PositiveInfinity",
            double d when double.IsNegativeInfinity(d) => "double.NegativeInfinity",
            double d                                   => d.ToString("R", CultureInfo.InvariantCulture) + "d",

            uint u   => u + "u",
            long l   => l + "L",
            ulong ul => ul + "UL",

            _ => Convert.ToString(constant.Value, CultureInfo.InvariantCulture)!,
        };
    }
    
    private static bool AcceptsNull(ITypeSymbol type) =>
        type.IsReferenceType ||
        type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

    private static bool TypeMatches(ITypeSymbol targetType, TypedConstant value)
    {
        // Unwrap Nullable<T> so [SmudgeDefault(5)] works on int? properties.
        if (targetType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            return value.IsNull || TypeMatches(nullable.TypeArguments[0], value);

        if (value.IsNull)
            return targetType.IsReferenceType;

        if (targetType.SpecialType == SpecialType.System_Object)
            return true;

        if (value.Type is null)
            return false;

        if (SymbolEqualityComparer.Default.Equals(
                targetType.WithNullableAnnotation(NullableAnnotation.None),
                value.Type.WithNullableAnnotation(NullableAnnotation.None)))
            return true;

        return IsImplicitNumericWidening(value.Type.SpecialType, targetType.SpecialType);
    }
    
    private static bool IsImplicitNumericWidening(SpecialType from, SpecialType to) => (from, to) switch
    {
        (SpecialType.System_SByte,  SpecialType.System_Int16  or SpecialType.System_Int32  or SpecialType.System_Int64  or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_Byte,   SpecialType.System_Int16  or SpecialType.System_UInt16 or SpecialType.System_Int32  or SpecialType.System_UInt32 or SpecialType.System_Int64  or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_Int16,  SpecialType.System_Int32  or SpecialType.System_Int64  or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_UInt16, SpecialType.System_Int32  or SpecialType.System_UInt32 or SpecialType.System_Int64  or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_Int32,  SpecialType.System_Int64  or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_UInt32, SpecialType.System_Int64  or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_Int64,  SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_UInt64, SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_Char,   SpecialType.System_UInt16 or SpecialType.System_Int32  or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
        (SpecialType.System_Single, SpecialType.System_Double) => true,
        _ => false,
    };
    
    private static bool IsCollectionType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String) return false;
        if (type is IArrayTypeSymbol) return true;
        // The property type may BE IEnumerable<T>; AllInterfaces doesn't include self.
        if (type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T) return true;
        return type.AllInterfaces.Any(i =>
            i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);
    }
    
    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
            return arrayType.ElementType;

        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } self)
            return self.TypeArguments[0];

        return type.AllInterfaces
            .FirstOrDefault(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
            ?.TypeArguments[0];
    }
}