using System.Text.Json;
using Smudge.Example;

var serializerOptions = new JsonSerializerOptions { WriteIndented = true };

var settings = new MySettings();

Console.WriteLine($"Volume: {settings.Volume}");
Console.WriteLine($"Songs: {string.Join(", ", settings.Songs)}");
Console.WriteLine($"Is Muted: {settings.IsMuted}");
Console.WriteLine($"Speaker Type: {settings.SpeakerType}");

Console.WriteLine($"Dirty: {settings.IsDirty}");
Console.WriteLine(JsonSerializer.Serialize(settings, serializerOptions));

settings.SpeakerType = SpeakerTypes.Headphones;

Console.WriteLine($"Dirty: {settings.IsDirty}");
Console.WriteLine(JsonSerializer.Serialize(settings, serializerOptions));