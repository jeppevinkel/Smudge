namespace Smudge.Example;

[Smudgeable(DirtyMode.PerProperty)]
public partial class MySettings
{
    [SmudgeDefault(50)]
    public partial int Volume { get; set; }
    
    [SmudgeDefault("When I'm There", "Duvet", "Heat Abnormal")]
    public partial List<string> Songs { get; set; }
    
    [SmudgeDefault(false)]
    public partial bool IsMuted { get; set; }
    
    [SmudgeDefault(SpeakerTypes.Speakers)]
    public partial SpeakerTypes SpeakerType { get; set; }
    
    public partial Dictionary<string, string> Categories { get; set; }

    public MySettings()
    {
        Categories = new Dictionary<string, string>([
            new KeyValuePair<string, string>("key1", "val1"),
            new KeyValuePair<string, string>("key2", "val2"),
        ]);
        WipeClean();
    }
}