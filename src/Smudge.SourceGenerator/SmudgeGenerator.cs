// using System;
// using System.Globalization;
// using System.Linq;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp;
//
// namespace Smudge.SourceGenerator;
//
// /// <summary>
// /// 
// /// </summary>
// [Generator]
// public class SmudgeGenerator : IIncrementalGenerator
// {
//     private const string SmudgeableAttributeName = "Smudge.SmudgeableAttribute";
//     private const string SmudgeDefaultAttributeName = "Smudge.SmudgeDefaultAttribute";
//     
//     public void Initialize(IncrementalGeneratorInitializationContext context)
//     {
//         throw new System.NotImplementedException();
//     }
//     
//     private static string FormatConstant(TypedConstant constant)
//     {
//         if (constant.IsNull) return "default";
//         if (constant.Kind == TypedConstantKind.Array) return "default"; // nested arrays unsupported
//
//         if (constant.Kind == TypedConstantKind.Enum)
//             return $"({constant.Type!.ToDisplayString(FullyQualifiedNullableFormat)})({Convert.ToString(constant.Value, CultureInfo.InvariantCulture)})";
//
//         if (constant.Kind == TypedConstantKind.Type)
//             return $"typeof({((ITypeSymbol)constant.Value!).ToDisplayString(FullyQualifiedNullableFormat)})";
//
//         return constant.Value switch
//         {
//             // FormatLiteral handles escaping ("a\"b", newlines, backslashes...)
//             string s => SymbolDisplay.FormatLiteral(s, quote: true),
//             char c   => SymbolDisplay.FormatLiteral(c, quote: true),
//             bool b   => b ? "true" : "false",
//
//             // Invariant culture (no "1,5" on de-DE build machines) + suffixes
//             // so float/double literals actually compile against the field type.
//             float f when float.IsNaN(f)                => "float.NaN",
//             float f when float.IsPositiveInfinity(f)   => "float.PositiveInfinity",
//             float f when float.IsNegativeInfinity(f)   => "float.NegativeInfinity",
//             float f                                    => f.ToString("R", CultureInfo.InvariantCulture) + "f",
//             double d when double.IsNaN(d)              => "double.NaN",
//             double d when double.IsPositiveInfinity(d) => "double.PositiveInfinity",
//             double d when double.IsNegativeInfinity(d) => "double.NegativeInfinity",
//             double d                                   => d.ToString("R", CultureInfo.InvariantCulture) + "d",
//
//             uint u   => u + "u",
//             long l   => l + "L",
//             ulong ul => ul + "UL",
//
//             _ => Convert.ToString(constant.Value, CultureInfo.InvariantCulture)!,
//         };
//     }
//     
//     private static bool AcceptsNull(ITypeSymbol type) =>
//         type.IsReferenceType ||
//         type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
//     
//     private static bool TypeMatches(ITypeSymbol targetType, TypedConstant value)
//     {
//         // Unwrap Nullable<T> so [SmudgeDefault(5)] works on int? properties.
//         if (targetType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
//             return value.IsNull || TypeMatches(nullable.TypeArguments[0], value);
//
//         if (value.IsNull)
//             return targetType.IsReferenceType;
//
//         if (targetType.SpecialType == SpecialType.System_Object)
//             return true;
//
//         if (value.Type is null)
//             return false;
//
//         if (SymbolEqualityComparer.Default.Equals(
//                 targetType.WithNullableAnnotation(NullableAnnotation.None),
//                 value.Type.WithNullableAnnotation(NullableAnnotation.None)))
//             return true;
//
//         // Allow implicit numeric widening: [SmudgeDefault(1)] on long/double etc.
//         return IsImplicitNumericWidening(value.Type.SpecialType, targetType.SpecialType);
//     }
//     
//     private static bool IsImplicitNumericWidening(SpecialType from, SpecialType to) => (from, to) switch
//     {
//         (SpecialType.System_SByte,  SpecialType.System_Int16  or SpecialType.System_Int32  or SpecialType.System_Int64  or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_Byte,   SpecialType.System_Int16  or SpecialType.System_UInt16 or SpecialType.System_Int32  or SpecialType.System_UInt32 or SpecialType.System_Int64  or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_Int16,  SpecialType.System_Int32  or SpecialType.System_Int64  or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_UInt16, SpecialType.System_Int32  or SpecialType.System_UInt32 or SpecialType.System_Int64  or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_Int32,  SpecialType.System_Int64  or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_UInt32, SpecialType.System_Int64  or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_Int64,  SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_UInt64, SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_Char,   SpecialType.System_UInt16 or SpecialType.System_Int32  or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal) => true,
//         (SpecialType.System_Single, SpecialType.System_Double) => true,
//         _ => false,
//     };
//     
//     private static bool IsCollectionType(ITypeSymbol type)
//     {
//         if (type.SpecialType == SpecialType.System_String) return false;
//         if (type is IArrayTypeSymbol) return true;
//         // The type itself may *be* IEnumerable<T>; AllInterfaces doesn't include self.
//         if (type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T) return true;
//         return type.AllInterfaces.Any(i =>
//             i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);
//     }
//
//     private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
//     {
//         if (type is IArrayTypeSymbol arrayType)
//             return arrayType.ElementType;
//
//         if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } self)
//             return self.TypeArguments[0];
//
//         return type.AllInterfaces
//             .FirstOrDefault(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
//             ?.TypeArguments[0];
//     }
// }