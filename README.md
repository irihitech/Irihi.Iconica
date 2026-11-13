# IRIHI Iconica

English | [简体中文](README.zh-CN.md)

Avalonia icon libraries that ship popular open-source icon sets as **native, runtime-customizable controls** — no SVG files, no asset management, full type safety.

| Package | NuGet | Icons | Icon source |
|---|---|---|---|
| [Irihi.Iconica.IconPark](src/Irihi.Iconica.IconPark) | Apache-2.0 | 2,600+ | [ByteDance IconPark](https://github.com/bytedance/iconpark) |
| [Irihi.Iconica.Semi](src/Irihi.Iconica.Semi) | MIT | 500+ | [Semi Design Icons](https://github.com/DouyinFE/semi-design) |
| [Irihi.Iconica.Semi.Lab](src/Irihi.Iconica.Semi.Lab) | MIT | 84 | [Semi Design Icons Lab](https://github.com/DouyinFE/semi-design) |
| [Irihi.Iconica.TDesign](src/Irihi.Iconica.TDesign) | MIT | 2,300+ | [Tencent TDesign Icons](https://github.com/Tencent/tdesign-icons) |

## Why this library?

Instead of importing individual SVG files as static resources, every icon is generated as a strongly-typed Avalonia control that renders through `DrawingContext`. That gives you:

- **Runtime customization** — change colors, stroke width, line caps and joins from XAML or code, even at runtime
- **Multiple display modes** — IconPark supports `Line`, `Fill`, `TwoTone` and `MultiColor`; TDesign supports outline/filled variants; Semi main icons follow the text foreground with `Secondary`/`Gradient` accent slots
- **Type safety & IntelliSense** — every icon is a C# class with a predictable name
- **No asset overhead** — no SVG files to manage, no naming conflicts, everything is compiled into the library
- **Performance** — pre-parsed geometry data for efficient rendering

## Getting started

### Install

```bash
dotnet add package Irihi.Iconica.IconPark   # IconPark icons
dotnet add package Irihi.Iconica.Semi       # Semi Design icons (theme-aware)
dotnet add package Irihi.Iconica.Semi.Lab   # Semi Design lab icons (colorful, fixed palette)
dotnet add package Irihi.Iconica.TDesign    # TDesign icons
```

### Usage (XAML)

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:iconpark="https://irihi.tech/iconica/iconpark"
        xmlns:semiicon="https://irihi.tech/iconica/semi"
        xmlns:semilabicon="https://irihi.tech/iconica/semi/lab"
        xmlns:tdicon="https://irihi.tech/iconica/tdesign">
    <StackPanel>
        <!-- IconPark: basic icon -->
        <iconpark:Bear Width="24" Height="24" />

        <!-- IconPark: runtime customization -->
        <iconpark:Bear Width="48" Height="48"
                       Mode="TwoTone"
                       OuterStroke="#333333"
                       OuterFill="#2F88FF"
                       InnerStroke="White"
                       InnerFill="#43CCF8"
                       StrokeWidth="4"
                       LineCap="Round" />

        <!-- Semi icons: follow the text foreground by default -->
        <semiicon:Bell />
        <semiicon:AiBellLevel2 Secondary="#A647FF" />
        <semiicon:AiBellLevel3 />
        <!-- Semi lab icons keep their designed multi-color palette -->
        <semilabicon:Anchor />

        <!-- TDesign icons -->
        <tdicon:AddCircle />
        <tdicon:AddCircleFilled />
        <tdicon:User Mode="FilledMultiple" />
    </StackPanel>
</Window>
```

For per-package details, see the package READMEs:
- [Irihi.Iconica.IconPark/README.md](src/Irihi.Iconica.IconPark/README.md)
- [Irihi.Iconica.Semi/README.md](src/Irihi.Iconica.Semi/README.md)
- [Irihi.Iconica.Semi.Lab/README.md](src/Irihi.Iconica.Semi.Lab/README.md)
- [Irihi.Iconica.TDesign/README.md](src/Irihi.Iconica.TDesign/README.md)

## Repository layout

```
src/
├── Irihi.Iconica.Core/            # Shared core (icon base types)
├── Irihi.Iconica.IconPark/        # IconPark package
├── Irihi.Iconica.Semi/            # Semi package (main icons)
├── Irihi.Iconica.Semi.Lab/        # Semi lab package (colorful icons)
├── Irihi.Iconica.TDesign/         # TDesign package
├── IconGenerator.IconPark/        # Generates IconPark icon classes from SVG
├── IconGenerator.Semi/            # Generates Semi icon classes from SVG
└── IconGenerator.TDesign/         # Generates TDesign icon classes from SVG
demo/
├── Irihi.Iconica.Demo/            # Icon gallery demo
├── Irihi.Iconica.Demo.Desktop/    # Desktop demo host
└── Irihi.Iconica.Demo.Browser/    # Browser demo host
external/
├── iconpark/                      # IconPark source (git submodule)
├── semi-design/                   # Semi Design source (git submodule)
└── tdesign-icons/                 # TDesign Icons source (git submodule)
```

## Building from source

### Prerequisites

- .NET 8.0 SDK or later
- Git

### Steps

```bash
git clone https://github.com/irihitech/Irihi.Iconica.git
cd Irihi.Iconica

# Initialize the icon source submodules
git submodule update --init

# Build the solution
dotnet build Irihi.Iconica.slnx
```

To regenerate icon classes after updating a submodule:

```bash
dotnet run --project src/IconGenerator.IconPark -f net10.0
dotnet run --project src/IconGenerator.Semi -f net10.0
dotnet run --project src/IconGenerator.TDesign -f net10.0
```

### Run the demo

```bash
dotnet run --project demo/Irihi.Iconica.Demo.Desktop
```

The demo provides a searchable gallery of all icons with live customization options.

## License

This repository is licensed under the **MIT License** — see [LICENSE](LICENSE).

The `Irihi.Iconica.IconPark` package is the exception: it stays under the
**Apache License 2.0** to match its upstream license — see
[src/Irihi.Iconica.IconPark/LICENSE](src/Irihi.Iconica.IconPark/LICENSE).

This product includes icons from:

- **IconPark** (https://github.com/bytedance/iconpark), Copyright 2019-present Bytedance Inc., Apache License 2.0
- **Semi Design** (https://github.com/DouyinFE/semi-design), Copyright (c) 2020-present ByteDance Inc., MIT License
- **TDesign Icons** (https://github.com/Tencent/tdesign-icons), Copyright (c) 2021-present TDesign, MIT License

Attribution notices are included in each package's `NOTICE` file. Generated code and libraries: Copyright © 2025 IRIHI Technology Co., Ltd.
