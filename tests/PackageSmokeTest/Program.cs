using Smudge;

Console.WriteLine("Smoke test starting");

var settings = new TestSettings();
if (settings.IsDirty) throw new Exception("Fresh instance should be clean");
settings.Volume = 11;
if (!settings.IsDirty) throw new Exception("Change should mark dirty");
settings.WipeClean();
if (settings.IsDirty) throw new Exception("WipeClean should reset");

Console.WriteLine("Smoke test passed");

[Smudgeable]
public partial class TestSettings
{
    [SmudgeDefault(50)]
    public partial int Volume { get; set; }
}