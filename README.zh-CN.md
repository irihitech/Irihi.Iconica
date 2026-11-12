# IRIHI Iconica

[English](README.md) | 简体中文

面向 Avalonia 的图标库：将流行的开源图标集封装为**原生、可运行时定制的控件**——无需 SVG 文件、无需资源管理、完全类型安全。

| 包 | NuGet | 图标数 | 图标来源 |
|---|---|---|---|
| [Irihi.Iconica.IconPark](src/Irihi.Iconica.IconPark) | Apache-2.0 | 2600+ | [字节跳动 IconPark](https://github.com/bytedance/iconpark) |
| [Irihi.Iconica.Semi](src/Irihi.Iconica.Semi) | MIT | 500+ | [Semi Design Icons](https://github.com/DouyinFE/semi-design) |
| [Irihi.Iconica.Semi.Lab](src/Irihi.Iconica.Semi.Lab) | MIT | 84 | [Semi Design Icons Lab](https://github.com/DouyinFE/semi-design) |
| [Irihi.Iconica.TDesign](src/Irihi.Iconica.TDesign) | MIT | 2300+ | [腾讯 TDesign Icons](https://github.com/Tencent/tdesign-icons) |

## 为什么选择这个库？

不同于传统方案（将单个 SVG 文件作为静态资源导入），本库把每个图标生成为强类型 Avalonia 控件，通过 `DrawingContext` 渲染。由此带来：

- **运行时定制**——可在 XAML 或代码中随时修改颜色、描边宽度、线帽与连接方式
- **多种显示模式**——IconPark 支持 `Line`、`Fill`、`TwoTone`、`MultiColor`；TDesign 支持描边/填充等多种变体；Semi 主包图标默认跟随文字前景色，双色/渐变图标提供 `Secondary`/`Gradient` 强调槽位
- **类型安全与智能提示**——每个图标都是命名可预期的 C# 类，IDE 完全智能提示
- **零资源管理负担**——没有 SVG 文件需要维护、无命名冲突，全部编译进库内
- **性能优化**——预解析的几何数据，渲染高效

## 快速开始

### 安装

```bash
dotnet add package Irihi.Iconica.IconPark   # IconPark 图标
dotnet add package Irihi.Iconica.Semi       # Semi Design 图标（跟随主题前景色）
dotnet add package Irihi.Iconica.Semi.Lab   # Semi Design lab 图标（彩色固定配色）
dotnet add package Irihi.Iconica.TDesign    # TDesign 图标
```

### 用法（XAML）

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:iconpark="https://irihi.tech/iconica/iconpark"
        xmlns:semiicon="https://irihi.tech/iconica/semi"
        xmlns:semilabicon="https://irihi.tech/iconica/semi/lab"
        xmlns:tdicon="https://irihi.tech/iconica/tdesign">
    <StackPanel>
        <!-- IconPark：基础图标 -->
        <iconpark:Bear Width="24" Height="24" />

        <!-- IconPark：运行时定制 -->
        <iconpark:Bear Width="48" Height="48"
                       Mode="TwoTone"
                       OuterStroke="#333333"
                       OuterFill="#2F88FF"
                       InnerStroke="White"
                       InnerFill="#43CCF8"
                       StrokeWidth="4"
                       LineCap="Round" />

        <!-- Semi 图标：默认跟随文字前景色 -->
        <semiicon:Bell />
        <semiicon:AiBellLevel2 Secondary="#A647FF" />
        <semiicon:AiBellLevel3 />
        <!-- Semi lab 图标：保留设计的多色配色 -->
        <semilabicon:Anchor />

        <!-- TDesign 图标 -->
        <tdicon:AddCircle />
        <tdicon:AddCircleFilled />
        <tdicon:User Mode="FilledMultiple" />
    </StackPanel>
</Window>
```

各包的详细说明见对应 README：
- [Irihi.Iconica.IconPark/README.md](src/Irihi.Iconica.IconPark/README.md)
- [Irihi.Iconica.Semi/README.md](src/Irihi.Iconica.Semi/README.md)
- [Irihi.Iconica.Semi.Lab/README.md](src/Irihi.Iconica.Semi.Lab/README.md)
- [Irihi.Iconica.TDesign/README.md](src/Irihi.Iconica.TDesign/README.md)

## 仓库结构

```
src/
├── Irihi.Iconica.Core/            # 共享核心（图标基类）
├── Irihi.Iconica.IconPark/        # IconPark 包
├── Irihi.Iconica.Semi/            # Semi 包（主包图标）
├── Irihi.Iconica.Semi.Lab/        # Semi lab 包（彩色图标）
├── Irihi.Iconica.TDesign/         # TDesign 包
├── IconGenerator.IconPark/        # 从 SVG 生成 IconPark 图标类
├── IconGenerator.Semi/            # 从 SVG 生成 Semi 图标类
└── IconGenerator.TDesign/         # 从 SVG 生成 TDesign 图标类
demo/
├── Irihi.Iconica.Demo/            # 图标展示 Demo
├── Irihi.Iconica.Demo.Desktop/    # 桌面端 Demo 宿主
└── Irihi.Iconica.Demo.Browser/    # 浏览器端 Demo 宿主
external/
├── iconpark/                      # IconPark 源码（git submodule）
├── semi-design/                   # Semi Design 源码（git submodule）
└── tdesign-icons/                 # TDesign Icons 源码（git submodule）
```

## 从源码构建

### 环境要求

- .NET 8.0 SDK 或更高版本

### 步骤

```bash
git clone https://github.com/irihitech/Irihi.Iconica.git
cd Irihi.Iconica

# 初始化图标源码子模块
git submodule update --init

# 构建解决方案
dotnet build Irihi.Iconica.slnx
```

更新子模块后，如需重新生成图标类：

```bash
dotnet run --project src/IconGenerator.IconPark -f net10.0
dotnet run --project src/IconGenerator.Semi -f net10.0
dotnet run --project src/IconGenerator.TDesign -f net10.0
```

### 运行 Demo

```bash
dotnet run --project demo/Irihi.Iconica.Demo.Desktop
```

Demo 提供全部图标的可搜索画廊，并支持实时定制预览。

## 许可证

本仓库采用 **MIT License** 许可——详见 [LICENSE](LICENSE)。

`Irihi.Iconica.IconPark` 包是例外：为与上游协议保持一致，该包沿用 **Apache License 2.0**——详见 [src/Irihi.Iconica.IconPark/LICENSE](src/Irihi.Iconica.IconPark/LICENSE)。

本产品包含以下第三方图标：

- **IconPark**（https://github.com/bytedance/iconpark），Copyright 2019-present Bytedance Inc.，Apache License 2.0
- **Semi Design**（https://github.com/DouyinFE/semi-design），Copyright (c) 2020-present ByteDance Inc.，MIT License
- **TDesign Icons**（https://github.com/Tencent/tdesign-icons），Copyright (c) 2021-present TDesign，MIT License

各包随附的 `NOTICE` 文件中包含上述归因声明。生成代码与库：Copyright © 2025 IRIHI Technology Co., Ltd.（上海铱泓科技服务有限公司）
