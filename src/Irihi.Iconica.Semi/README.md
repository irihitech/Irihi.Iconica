# IRIHI ICONICA - Semi

A flexible and customizable icon library for Avalonia applications, built on top of the
[Semi Design Icons](https://github.com/DouyinFE/semi-design) collection (icons by ByteDance's Semi team).

## About

Irihi.Iconica.Semi is a .NET library that provides the 523 main Semi Design icons
(`semi-icons`) as theme-aware Avalonia controls following the text foreground.
The colorful illustrative "lab" set ships separately as
[Irihi.Iconica.Semi.Lab](../Irihi.Iconica.Semi.Lab).

## Key Features

- **523 main icons** generated as native Avalonia controls from Semi's SVG source
- **Theme-aware**: icons inherit the text foreground by default; two-tone (AI level 2) and
  gradient (AI level 3) icons expose `Secondary` and `Gradient` brushes
- **Runtime customization**: change `Primary` / `Secondary` / `Gradient`, stroke width, line cap and join
- **Type-safe**: every icon is a strongly-typed C# class
- **Avalonia native**: icons are rendered as Avalonia controls using `DrawingContext`

## Usage

```xml
<StackPanel Orientation="Horizontal" Spacing="8">
    <!-- Main set: follows the text foreground -->
    <semiicon:Bell />

    <!-- Customize -->
    <semiicon:Bell Primary="#0077FA" />
    <semiicon:AiBellLevel2 Secondary="#A647FF" />
    <semiicon:AiBellLevel3 />
</StackPanel>
```

## Regenerating Icons

The icon sources are checked out as a git submodule under `external/semi-design`. After updating
the submodule, regenerate the icon classes:

```bash
dotnet run --project src/IconGenerator.Semi -f net10.0
```

This regenerates:

- `src/Irihi.Iconica.Semi/Generated/*.cs` — main-set icons (inherit `SemiIconBase`)
- `src/Irihi.Iconica.Semi.Lab/Generated/*.cs` — lab icons (inherit `SemiLabIconBase`)
- `demo/Irihi.Iconica.Demo/Models/IconInfo.Semi.Generated.cs` — demo icon list

## License

The icon designs are copyrighted by the Semi Design authors; the generated code is copyrighted by
IRIHI Technology. See `LICENSE` and `NOTICE` for details.
