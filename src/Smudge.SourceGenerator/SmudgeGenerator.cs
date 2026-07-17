using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Smudge.SourceGenerator;


/// <summary>
/// Smudgeable class generator.
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