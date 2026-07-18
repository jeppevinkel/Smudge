# Smudge

Dirty-tracking for C# classes via source generation. Mark a class with `[Smudgeable]` and Smudge will generate the plumbing that lets you know when a property has been changed and reset it back to "clean" with a single call.

## Installation

```shell
dotnet add package Smudge
```

Or add it to your project file:

```xml
<PackageReference Include="Smudge" Version="*" />
```

## Quick start

Declare a `partial` class, apply `[Smudgeable]`, and make each property you want tracked `partial`:

```csharp
using Smudge;

[Smudgeable]
public partial class UserSettings
{
    public partial string Theme { get; set; }
    public partial bool Notifications { get; set; }
}
```

The source generator implements `ISmudgeable` for you:

```csharp
var settings = new UserSettings();

Console.WriteLine(settings.IsDirty); // False

settings.Theme = "dark";

Console.WriteLine(settings.IsDirty); // True

settings.WipeClean();

Console.WriteLine(settings.IsDirty); // False
```

## Default values

Use `[SmudgeDefault]` to set the initial value of a tracked property. The property returns to this value after `WipeClean()`.

```csharp
[Smudgeable]
public partial class AudioSettings
{
    [SmudgeDefault(50)]
    public partial int Volume { get; set; }       // starts at 50

    [SmudgeDefault("guest")]
    public partial string Username { get; set; }  // starts as "guest"
}
```

Collection properties accept multiple values as elements:

```csharp
[Smudgeable]
public partial class LocaleSettings
{
    [SmudgeDefault("en", "de")]
    public partial List<string> Languages { get; set; } // starts as ["en", "de"]
}
```

## Per-property tracking

By default, all changes are collapsed into a single `IsDirty` flag. Use `DirtyMode.PerProperty` to track each property individually. The class then implements `IPerPropertySmudgeable`, which adds `IsPropertyDirty` and `DirtyProperties`.

```csharp
using Smudge;

[Smudgeable(DirtyMode.PerProperty)]
public partial class GameSettings
{
    [SmudgeDefault(50)]
    public partial int Volume { get; set; }

    public partial string? ServerRegion { get; set; }
}
```

```csharp
var settings = new GameSettings();

settings.Volume = 80;

Console.WriteLine(settings.IsDirty);                         // True
Console.WriteLine(settings.IsPropertyDirty("Volume"));       // True
Console.WriteLine(settings.IsPropertyDirty("ServerRegion")); // False

foreach (string name in settings.DirtyProperties)
{
    Console.WriteLine(name); // "Volume"
}

settings.WipeClean();

Console.WriteLine(settings.IsDirty); // False
```

> **Note:** Per-property tracking supports a maximum of 64 tracked properties per class.

## Constraints

- The class must be declared `partial`.
- Tracked properties must be `partial` and have both a getter and a setter.
- non-nullable reference properties without `[SmudgeDefault]` start as null. Supply a default via the attribute, or assign in a constructor and call `WipeClean()`.
- Static properties and indexers are not tracked.
- Generic classes and nested classes are not supported.

## Diagnostics

| Code     | Meaning                                                       |
|----------|---------------------------------------------------------------|
| SMDG001  | `[SmudgeDefault]` argument count does not match property type |
| SMDG002  | `[SmudgeDefault]` value type does not match property type     |
| SMDG003  | Class must be `partial`                                       |
| SMDG004  | Generic or nested classes are not supported                   |
| SMDG005  | Property must have both a getter and a setter                 |
| SMDG006  | Invalid `DirtyMode` value                                     |
| SMDG007  | Too many tracked properties (maximum 64 in PerProperty mode)  |
