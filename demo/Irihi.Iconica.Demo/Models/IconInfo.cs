using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace Irihi.Iconica.Demo.Models;

public partial class IconInfo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryChinese { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public List<string> Tag { get; set; } = [];
    public bool Rtl { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Func<Control>? Creator { get; set; }
    public List<string> Keywords { get; set; } = [];
    public Type? IconType { get; set; }

    /// <summary>TDesign 图标是否 Filled 变体（类名以 Filled 结尾）。</summary>
    public static bool IsFilled(IconInfo icon) => icon.ClassName.EndsWith("Filled", StringComparison.Ordinal);

    /// <summary>TDesign 图标是否 Outline 变体。</summary>
    public static bool IsOutline(IconInfo icon) => !IsFilled(icon);

    /// <summary>Semi 图标是否 Lab（彩色插画）族。</summary>
    public static bool IsSemiLab(IconInfo icon) => icon.Category == "Lab";

    /// <summary>Semi 图标是否 AI 系列（单色/双色/渐变）。</summary>
    public static bool IsSemiAi(IconInfo icon)
        => icon.Category is "One Color" or "Two Color" or "Multiple Color";

    /// <summary>Semi 图标是否线条风变体（类名以 Stroked 结尾，形状仍为填充实现）。
    /// AI 系列的 AIStrokedLevel* 属于 AI 族，不算 Stroked 族。</summary>
    public static bool IsSemiStroked(IconInfo icon)
        => icon.ClassName.EndsWith("Stroked", StringComparison.Ordinal) && !IsSemiAi(icon);

    /// <summary>Semi 图标是否 Fill（实心默认）族。</summary>
    public static bool IsSemiFill(IconInfo icon)
        => !IsSemiLab(icon) && !IsSemiAi(icon) && !IsSemiStroked(icon);
}