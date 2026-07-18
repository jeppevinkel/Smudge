using System.Globalization;

namespace Smudge.SourceGenerator.Tests;

public class SmudgeDefaultTests
{
    [Theory]
    [InlineData("int", "42")]
    [InlineData("double", "1.5d")]
    [InlineData("double", "double.NaN")]
    [InlineData("double", "double.PositiveInfinity")]
    [InlineData("double", "double.NegativeInfinity")]
    [InlineData("string", "\"hello \\\"quoted\\\"\"")]
    [InlineData("string", "\"back\\\\slash\"")]
    [InlineData("string", "\"line1\\nline2\"")]
    [InlineData("char", "'x'")]
    [InlineData("float", "1.5f")]
    [InlineData("long", "1")]            // widening: int literal → long property
    [InlineData("double", "1")]          // widening: int literal → double property
    [InlineData("decimal", "1")]         // widening: int literal → decimal property
    [InlineData("int?", "5")]            // Nullable<T> unwrap
    [InlineData("string?", "null")]      // params-null crash guard (see TESTPLAN crash regressions)
    [InlineData("long",  "5000000000L")] // exceeds int range, tests L suffix for long
    [InlineData("uint",  "5u")]
    [InlineData("ulong", "5UL")]
    [InlineData("bool",  "true")]
    public void SmudgeDefault_ValidValue_Compiles(string type, string value)
    {
        var result = TestHelper.RunGenerator($$"""
                                               using Smudge;
                                               namespace Demo;

                                               [Smudgeable]
                                               public partial class Settings
                                               {
                                                   [SmudgeDefault({{value}})]
                                                   public partial {{type}} Setting { get; set; }
                                               }
                                               """, out var output);

        Assert.Empty(result.Diagnostics);
        TestHelper.AssertNoCompilationErrors(output);
    }
    
    [Fact]
    public void SmudgeDefault_Floats_CultureInvariant()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var result = TestHelper.RunGenerator("""
                                               using Smudge;
                                               namespace Demo;

                                               [Smudgeable]
                                               public partial class Settings
                                               {
                                                   [SmudgeDefault(1.5)]
                                                   public partial double Setting { get; set; }
                                               }
                                               """, out var output);
            Assert.Empty(result.Diagnostics);
            TestHelper.AssertNoCompilationErrors(output);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}