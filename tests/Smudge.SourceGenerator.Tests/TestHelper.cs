using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Smudge.SourceGenerator.Tests;

internal static class TestHelper
{
    // C# 13 = minimum toolchain we document. Must be set on the DRIVER too:
    // generated trees are parsed with the driver's options, and they contain
    // partial properties.
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.CSharp13);
    
    public static GeneratorRunResult RunGenerator(string source, out Compilation output)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "SmudgeTestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, ParseOptions)],
            references:
            [
                ..Net100.References.All,
                MetadataReference.CreateFromFile(typeof(SmudgeableAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ISmudgeable).Assembly.Location),
            ],
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new SmudgeGenerator().AsSourceGenerator()],
            parseOptions: ParseOptions,
            driverOptions: new GeneratorDriverOptions(
                IncrementalGeneratorOutputKind.None,
                trackIncrementalGeneratorSteps: true)); // used by the caching tests later

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out output, out _);
        return driver.GetRunResult().Results.Single();
    }
    
    public static void AssertNoCompilationErrors(Compilation output)
    {
        var errors = output.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error
                        || (d.Severity == DiagnosticSeverity.Warning
                            && d.Location.SourceTree?.FilePath.EndsWith(".g.cs") == true))
            .ToList();

        if (errors.Count > 0)
            Assert.Fail(
                "Generated compilation has errors:\n" +
                string.Join("\n", errors.Select(e => $"  {e}")));
    }
}