namespace Smudge.SourceGenerator.Tests;

public class HappyPathTests
{
    [Fact]
    public void AggregatedMode_MultipleProperties_Compiles()
    {
        var result = TestHelper.RunGenerator("""
                                             using Smudge;

                                             namespace Demo;

                                             [Smudgeable(DirtyMode.Aggregated)]
                                             public partial class Settings
                                             {
                                                 public partial int Volume { get; set; }
                                                 public partial string? Nickname { get; set; }
                                                 public partial bool Enabled { get; set; }
                                             }
                                             """, out var output);

        Assert.Empty(result.Diagnostics);
        TestHelper.AssertNoCompilationErrors(output);
        
        var generated = Assert.Single(result.GeneratedSources).SourceText.ToString();
        Assert.Contains("string? ", generated);
    }
    
    [Fact]
    public void PerPropertyMode_MultipleProperties_Compiles()
    {
        var result = TestHelper.RunGenerator("""
                                             using Smudge;

                                             namespace Demo;

                                             [Smudgeable(DirtyMode.PerProperty)]
                                             public partial class Settings
                                             {
                                                 public partial int Volume { get; set; }
                                                 public partial string? Nickname { get; set; }
                                                 public partial bool Enabled { get; set; }
                                             }
                                             """, out var output);

        Assert.Empty(result.Diagnostics);
        TestHelper.AssertNoCompilationErrors(output);

        // PerProperty must implement the per-property interface
        var generated = Assert.Single(result.GeneratedSources).SourceText.ToString();
        Assert.Contains("global::Smudge.IPerPropertySmudgeable", generated);
    }
    
    [Fact]
    public void BareSmudgeable_DefaultsToAggregated()
    {
        var result = TestHelper.RunGenerator("""
                                             using Smudge;

                                             namespace Demo;

                                             [Smudgeable]
                                             public partial class Settings
                                             {
                                                 public partial int Volume { get; set; }
                                             }
                                             """, out var output);

        Assert.Empty(result.Diagnostics);
        TestHelper.AssertNoCompilationErrors(output);

        // Default mode is Aggregated: base interface only, no per-property machinery
        var generated = Assert.Single(result.GeneratedSources).SourceText.ToString();
        Assert.Contains("global::Smudge.ISmudgeable", generated);
        Assert.DoesNotContain("IPerPropertySmudgeable", generated);
        Assert.DoesNotContain("_dirtyBits", generated);
    }
    
    [Fact]
    public void ClassInNamespace_Compiles_WithNamespacedHintName()
    {
        var result = TestHelper.RunGenerator("""
                                             using Smudge;

                                             namespace Some.Deeply.Nested.Space;

                                             [Smudgeable]
                                             public partial class Settings
                                             {
                                                 public partial int Volume { get; set; }
                                             }
                                             """, out var output);

        Assert.Empty(result.Diagnostics);
        TestHelper.AssertNoCompilationErrors(output);

        var source = Assert.Single(result.GeneratedSources);
        Assert.Equal("Some.Deeply.Nested.Space.Settings.g.cs", source.HintName);
        Assert.Contains("namespace Some.Deeply.Nested.Space;", source.SourceText.ToString());
    }
    
    [Fact]
    public void ClassInGlobalNamespace_Compiles()
    {
        var result = TestHelper.RunGenerator("""
                                             using Smudge;

                                             [Smudgeable]
                                             public partial class GlobalSettings
                                             {
                                                 public partial int Volume { get; set; }
                                             }
                                             """, out var output);

        Assert.Empty(result.Diagnostics);
        TestHelper.AssertNoCompilationErrors(output);

        var source = Assert.Single(result.GeneratedSources);
        Assert.Equal("GlobalSettings.g.cs", source.HintName);
        Assert.DoesNotContain("namespace ", source.SourceText.ToString());
    }
}