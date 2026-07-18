namespace Smudge.Tests;

[Smudgeable]
public partial class AggregatedSettings
{
    [SmudgeDefault(50)]
    public partial int Volume { get; set; }

    [SmudgeDefault("guest")]
    public partial string Username { get; set; }

    public partial bool Enabled { get; set; }
}

[Smudgeable(DirtyMode.PerProperty)]
public partial class PerPropertySettings
{
    [SmudgeDefault(50)]
    public partial int Volume { get; set; }

    public partial string? Nickname { get; set; }

    [SmudgeDefault("en", "de")]
    public partial List<string> Languages { get; set; }
}