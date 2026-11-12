# IRIHI ICONICA - Semi Lab

A colorful illustrative icon library for Avalonia applications, built on top of the
[Semi Design Icons Lab](https://github.com/DouyinFE/semi-design) set (icons by ByteDance's Semi team).

## About

Irihi.Iconica.Semi.Lab provides the 84 "lab" icons of the Semi Design icon family —
component/concept illustrations with a designed multi-color palette (grayscale skeletons
with blue, yellow, red and green accents). Unlike the theme-aware
[Irihi.Iconica.Semi](../Irihi.Iconica.Semi) main set, lab icons keep their fixed palette:
the colors are part of the design.

## Key Features

- **84 lab icons** rendered as native Avalonia controls
- **Faithful fixed palette**: per-path fills, stroke widths, caps and joins exactly as designed
- **Non-24 source view boxes** handled via uniform scaling and centering
- **Type-safe**: every icon is a strongly-typed C# class
- **Avalonia native**: icons are rendered using `DrawingContext`

## Usage

```xml
<StackPanel Orientation="Horizontal" Spacing="8">
    <semilabicon:Anchor />
    <semilabicon:DatePicker />
    <semilabicon:Carousel />
</StackPanel>
```

## Regenerating Icons

The icon sources are checked out as a git submodule under `external/semi-design`. After updating
the submodule, regenerate the icon classes:

```bash
dotnet run --project src/IconGenerator.Semi -f net10.0
```

(The same generator emits both this package and `Irihi.Iconica.Semi`.)

## License

The icon designs are copyrighted by the Semi Design authors; the generated code is copyrighted by
IRIHI Technology. See `LICENSE` and `NOTICE` for details.
