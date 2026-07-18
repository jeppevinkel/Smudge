namespace Smudge.Tests;

public class PerPropertyBehaviorTests
{
    [Fact]
    public void PropertiesTrackIndependently()
    {
        var s = new PerPropertySettings { Nickname = "neo" };

        Assert.True(s.IsDirty);
        Assert.True(s.IsPropertyDirty(nameof(PerPropertySettings.Nickname)));
        Assert.False(s.IsPropertyDirty(nameof(PerPropertySettings.Volume)));
    }

    [Fact]
    public void DirtyProperties_ContainsExactlyTheChangedOnes()
    {
        var s = new PerPropertySettings { Nickname = "neo", Volume = 11 };
        Assert.Equal(["Volume", "Nickname"], s.DirtyProperties); // declaration order
    }

    [Fact]
    public void UnknownPropertyName_Throws()
    {
        var s = new PerPropertySettings();
        var ex = Assert.Throws<ArgumentException>(() => s.IsPropertyDirty("Nope"));
        Assert.Equal("propertyName", ex.ParamName);
    }

    [Fact]
    public void WipeClean_ClearsAllBits()
    {
        var s = new PerPropertySettings { Nickname = "neo", Volume = 11 };
        s.WipeClean();

        Assert.False(s.IsDirty);
        Assert.Empty(s.DirtyProperties);
    }
    
    [Fact]
    public void PerPropertyInterface_IsAlsoAggregateInterface()
    {
        ISmudgeable s = new PerPropertySettings();   // inheritance contract
        Assert.False(s.IsDirty);
    }
}