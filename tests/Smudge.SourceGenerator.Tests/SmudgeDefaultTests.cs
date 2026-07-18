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
    [InlineData("char", "'x'")]
    [InlineData("float", "1.5f")]
    [InlineData("long", "1")]           // widening: int literal → long property
    [InlineData("int?", "5")]           // Nullable<T> unwrap
    [InlineData("string?", "null")]
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
}