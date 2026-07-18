namespace Smudge.Tests;

public class AggregatedBehaviorTests
{
    [Fact]
    public void FreshInstance_IsClean_AndDefaultsApplied()
    {
        var s = new AggregatedSettings();

        Assert.False(s.IsDirty);          // field initializers bypass the setter
        Assert.Equal(50, s.Volume);
        Assert.Equal("guest", s.Username);
        Assert.False(s.Enabled);          // no [SmudgeDefault] → default
    }

    [Fact]
    public void ChangingAnyProperty_MarksDirty()
    {
        var s = new AggregatedSettings { Volume = 11 };
        Assert.True(s.IsDirty);
    }

    [Fact]
    public void AssigningSameValue_DoesNotDirty()
    {
        var s = new AggregatedSettings();
        s.Volume = 50;                    // same as default
        s.Username = "guest";
        Assert.False(s.IsDirty);
    }

    [Fact]
    public void WipeClean_Resets_AndSameValueStaysClean()
    {
        var s = new AggregatedSettings { Volume = 11 };
        s.WipeClean();

        Assert.False(s.IsDirty);
        s.Volume = 11;                    // field already holds 11 -> no re-dirty
        Assert.False(s.IsDirty);
    }
    
    [Fact]
    public void UsableThroughInterface()
    {
        ISmudgeable s = new AggregatedSettings();
        ((AggregatedSettings)s).Volume = 11;

        Assert.True(s.IsDirty);
        s.WipeClean();
        Assert.False(s.IsDirty);
    }
}