# Source Generator Test Plan

Four categories, in priority order. Every test runs through `CSharpGeneratorDriver`
against an in-memory compilation (C# 13 parse options, also on the driver, since
generated trees use the driver's options).

## 1. Generated code compiles (highest value, write these first)

Assert `output.GetDiagnostics()` has no errors after running the generator.

### Happy paths
- [x] Aggregated mode: class with a few partial properties, no `[SmudgeDefault]`
- [x] PerProperty mode: same shape
- [x] Bare `[Smudgeable]` (constructor default → Aggregated)
- [x] Class in a namespace / class in the global namespace
- [ ] `internal` class (generated part must not conflict → no accessibility modifier emitted)
- [ ] Property accessibility mirrored (`internal partial int X { get; set; }`)
- [ ] Two smudgeable classes with the same name in different namespaces (hint-name collision)
- [ ] Class with zero partial properties (empty but valid output)

### Property types
- [ ] Primitives: int, bool, string
- [ ] `string?` and other nullable reference types (emitted type must carry the `?`)
- [ ] Nullable value types: `int?`
- [ ] Enums (user-defined, in another namespace, fully-qualified emission)
- [ ] Types whose names collide with usings (property type `Task` etc., global:: qualification)

### `[SmudgeDefault]` values, each must produce *compiling* initializers
- [x] string containing quotes, backslash, newline (escaping, `SymbolDisplay.FormatLiteral`)
- [x] char (must emit quoted `'c'`)
- [x] float / double (suffix: `1.5f`, `1.5d`; invariant culture, decimal separator!)
- [x] double.NaN / infinities
- [ ] long, uint, ulong (suffixes)
- [ ] bool, enum value
- [x] `[SmudgeDefault(null)]` on nullable property
- [x] Numeric widening: `[SmudgeDefault(1)]` on long / double / decimal property

### Collections
- [ ] `List<string>` with `[SmudgeDefault("a", "b")]` → collection expression
- [ ] Array property (`int[]`)
- [ ] Interface-typed: `IEnumerable<int>`, `IReadOnlyList<int>` (self-type detection,
      `AllInterfaces` doesn't include self!)
- [ ] Empty `[SmudgeDefault]` on a collection → `[]`
- [ ] string property is NOT treated as a collection

## 2. Diagnostics (assert ID **and** location; generator must still emit
      valid-as-possible output, no CS9248 cascades)

- [ ] SMDG001: wrong argument count, multiple values on non-collection property
- [ ] SMDG001 (or distinct message): zero values on non-collection property
- [ ] SMDG002: type mismatch, scalar (`[SmudgeDefault("x")]` on int)
- [ ] SMDG002: type mismatch, per collection element (wrong element among valid ones)
- [ ] SMDG002: `[SmudgeDefault(null)]` on non-nullable value type
- [ ] SMDG003: `[Smudgeable]` on non-partial class
- [ ] SMDG004: generic class / nested class (unsupported shapes)
- [ ] SMDG005: partial property without both get and set
- [ ] PerProperty mode with > 64 properties (bitmask limit), whichever behavior chosen
- [ ] Negative: `[SmudgeDefault]` on a property in a NON-smudgeable class → no output, no crash
- [ ] Negative: someone else's attribute named `SmudgeDefaultAttribute` in another
      namespace → ignored (fully-qualified matching)

## 3. Snapshot tests (Verify), lock the exact output shape

- [ ] One canonical Aggregated-mode class (several property types + defaults)
- [ ] One canonical PerProperty-mode class (IsPropertyDirty switch, DirtyProperties,
      bit indices stable)
- [ ] Attribute/interface emission if any post-init output exists

Keep these FEW, snapshots are for reviewing intentional emit changes,
not for covering cases (that's category 1).

## 4. Incremental pipeline behavior

- [ ] Unrelated syntax tree added → all tracked output steps report Cached/Unchanged
- [ ] Whitespace/comment edit inside the smudgeable class → still cached
      (model equality, not syntax identity)
- [ ] Real change (add a property) → output regenerated
- [ ] Guard: pipeline models contain no ISymbol/SyntaxNode/Compilation references
      (the caching test above is the practical enforcement)

## Crash regressions (must never take down the compilation)

- [ ] `[SmudgeDefault(null)]`, literal null binds to the params ARRAY itself;
      `TypedConstant.Values` on it throws without the IsNull guard
- [ ] Nested array argument `[SmudgeDefault(new[] { 1 })]`, fallback, not exception
- [ ] Attribute present but class declaration is syntactically broken (partial typing
      mid-edit, generator runs on incomplete code in the IDE constantly)

## Out of scope here (lives in Smudge.Tests)

Runtime behavior: IsDirty transitions, same-value assignment not dirtying,
MarkClean, DirtyProperties contents, interface polymorphism, defaults actually
applied at runtime.
