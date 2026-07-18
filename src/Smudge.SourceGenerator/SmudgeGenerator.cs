using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Smudge.SourceGenerator;


/// <summary>
/// Roslyn incremental source generator that implements dirty-tracking for classes marked with
/// <see cref="T:Smudge.SmudgeableAttribute"/>. For each such class the generator emits a partial
/// class that implements <see cref="T:Smudge.ISmudgeable"/> or
/// <see cref="T:Smudge.IPerPropertySmudgeable"/> depending on the chosen <see cref="T:Smudge.DirtyMode"/>.
/// </summary>
[Generator]
public class SmudgeGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Smudge.SmudgeableAttribute",
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, ct) => Transformer.Transform(ctx, ct));

        context.RegisterSourceOutput(classes, static (ctx, model) => Emitter.Emit(ctx, model));
    }
}