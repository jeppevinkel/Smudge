namespace Smudge.Tests;

public class CollectionSemanticsTests
{
    [Fact]
    public void EachInstance_GetsItsOwnDefaultCollection()
    {
        var a = new PerPropertySettings();
        var b = new PerPropertySettings();

        a.Languages.Add("fr");
        Assert.Equal(["en", "de"], b.Languages);   // no shared default instance
    }

    [Fact]
    public void MutatingCollectionContents_DoesNotDirty()
    {
        // Documented limitation: only ASSIGNMENT is tracked, not mutation.
        var s = new PerPropertySettings();
        s.Languages.Add("fr");
        Assert.False(s.IsDirty);
    }

    [Fact]
    public void ReassigningCollection_Dirties()
    {
        var s = new PerPropertySettings();
        s.Languages = ["fr"];
        Assert.True(s.IsDirty);
    }
}