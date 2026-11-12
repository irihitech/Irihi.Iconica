using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using IconGenerator.Semi;

// Repository root: explicit --root <path> wins; otherwise infer from build output location
// (src/IconGenerator.Semi/bin/{Config}/{TFM} -> 5 levels up).
var rootPath = args.Length >= 2 && args[0] == "--root"
    ? Path.GetFullPath(args[1])
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

var mainSvgPath = Path.Combine(rootPath, "external", "semi-design", "packages", "semi-icons", "src", "svgs");
var labSvgPath = Path.Combine(rootPath, "external", "semi-design", "packages", "semi-icons-lab", "src", "svgs");
var metaPath = Path.Combine(mainSvgPath, "meta.json");

var mainTargetPath = Path.Combine(rootPath, "src", "Irihi.Iconica.Semi", "Generated");
var labTargetPath = Path.Combine(rootPath, "src", "Irihi.Iconica.Semi.Lab", "Generated");
// Recreate the output directories so icons removed upstream never linger as stale files.
foreach (var target in new[] { mainTargetPath, labTargetPath })
{
    if (Directory.Exists(target)) Directory.Delete(target, true);
    Directory.CreateDirectory(target);
}

var categories = LoadCategories(metaPath);
var categoryChinese = CreateCategoryChinese();
var failures = new List<(string File, string Error)>();
var iconsInfo = new List<IconInfo>();
// Class names only need to be unique within a package (the two packages have separate xmlns).
var mainClassNames = new HashSet<string>(StringComparer.Ordinal);
var labClassNames = new HashSet<string>(StringComparer.Ordinal);

var mainFiles = Directory.GetFiles(mainSvgPath, "*.svg").OrderBy(f => f, StringComparer.Ordinal).ToArray();
var labFiles = Directory.GetFiles(labSvgPath, "*.svg").OrderBy(f => f, StringComparer.Ordinal).ToArray();

foreach (var svgFile in mainFiles) ProcessFile(svgFile, isLab: false);
foreach (var svgFile in labFiles) ProcessFile(svgFile, isLab: true);

var demoFile = Path.Combine(rootPath, "demo", "Irihi.Iconica.Demo", "Models", "IconInfo.Semi.Generated.cs");
var demoCode = GenerateDemoDocument(iconsInfo);
await File.WriteAllTextAsync(demoFile, demoCode);

Console.WriteLine($"Generated {mainFiles.Length} main icons -> {mainTargetPath}");
Console.WriteLine($"Generated {labFiles.Length} lab icons -> {labTargetPath}");
Console.WriteLine($"Generated demo info -> {demoFile}");
if (failures.Count > 0)
{
    Console.Error.WriteLine($"{failures.Count} icons failed to generate:");
    foreach (var (file, error) in failures)
        Console.Error.WriteLine($"  {Path.GetFileName(file)}: {error}");
    Environment.Exit(1);
}

void ProcessFile(string svgFile, bool isLab)
{
    try
    {
        var stem = Path.GetFileNameWithoutExtension(svgFile);
        var className = ToPascalCase(stem);
        var classNames = isLab ? labClassNames : mainClassNames;
        if (!classNames.Add(className))
            throw new InvalidOperationException($"Duplicate class name: {className}");

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(svgFile);
        var root = xmlDoc.DocumentElement!;

        var (viewBoxWidth, viewBoxHeight) = ParseViewBox(root.GetAttribute("viewBox"));

        var elements = new List<SvgElement>();
        WalkElements(root, elements);
        if (elements.Count == 0)
            throw new NotSupportedException("No drawable elements found.");

        var sourceCode = isLab
            ? GenerateLabDocument(className, elements, viewBoxWidth, viewBoxHeight)
            : GenerateMainDocument(className, elements, viewBoxWidth, viewBoxHeight);
        var targetDir = isLab ? labTargetPath : mainTargetPath;
        var targetPath = Path.Combine(targetDir, $"{className}.cs");
        File.WriteAllText(targetPath, sourceCode);

        string categoryEn;
        if (isLab)
        {
            categoryEn = "Lab";
        }
        else if (!categories.TryGetValue(stem, out var meta))
        {
            categoryEn = "Other";
        }
        else
        {
            categoryEn = meta;
        }

        var categoryCn = isLab ? "Lab" : categoryChinese.GetValueOrDefault(categoryEn, categoryEn);
        var title = SplitCamelCase(className.TrimStart('_'));

        iconsInfo.Add(new IconInfo
        {
            ClassName = className,
            Namespace = isLab ? "Lab" : "Icons",
            Name = stem,
            Category = categoryEn,
            CategoryCN = categoryCn,
            Title = title,
            Keywords = [stem, className, title],
        });
    }
    catch (Exception e)
    {
        failures.Add((svgFile, e.Message));
        Console.Error.WriteLine($"Failed: {svgFile}");
        Console.Error.WriteLine(e.Message);
    }
}

// Intermediate SVG shape representation, shared by both emitters.
void WalkElements(XmlElement parent, List<SvgElement> result)
{
    foreach (var node in parent.ChildNodes)
    {
        if (node is not XmlElement element) continue;

        switch (element.Name)
        {
            case "defs":
                // Gradient/clip definitions are not drawn; referenced gradients are
                // expressed through brush slots and clipPaths are 24x24 export noise.
                continue;
            case "g":
                WalkElements(element, result);
                continue;
            case "path":
            case "circle":
            case "ellipse":
            case "rect":
            case "line":
                result.Add(ParseShape(element));
                break;
        }
    }
}

SvgElement ParseShape(XmlElement e)
{
    var shape = new SvgElement
    {
        Kind = e.Name,
        Fill = e.GetAttribute("fill"),
        Stroke = e.GetAttribute("stroke"),
        StrokeWidth = e.GetAttribute("stroke-width"),
        LineCap = e.GetAttribute("stroke-linecap"),
        LineJoin = e.GetAttribute("stroke-linejoin"),
        FillRule = e.GetAttribute("fill-rule"),
        Transform = e.GetAttribute("transform"),
    };

    switch (e.Name)
    {
        case "path":
            shape.D = e.GetAttribute("d");
            break;
        case "circle":
        case "ellipse":
            shape.Cx = e.GetAttribute("cx");
            shape.Cy = e.GetAttribute("cy");
            shape.R = e.GetAttribute("r");
            shape.Rx = e.GetAttribute("rx");
            shape.Ry = e.GetAttribute("ry");
            break;
        case "rect":
            shape.X = e.GetAttribute("x");
            shape.Y = e.GetAttribute("y");
            shape.Width = e.GetAttribute("width");
            shape.Height = e.GetAttribute("height");
            shape.Rx = e.GetAttribute("rx");
            shape.Ry = e.GetAttribute("ry");
            break;
        case "line":
            shape.X1 = e.GetAttribute("x1");
            shape.Y1 = e.GetAttribute("y1");
            shape.X2 = e.GetAttribute("x2");
            shape.Y2 = e.GetAttribute("y2");
            break;
    }

    return shape;
}

// --- Main set: colors map to brush slots (0: Primary, 1: Secondary, 2: Gradient) ---

static int MapMainFill(string? fill) => fill switch
{
    null or "" or "none" => -1,
    // Theme colors (including near-black brand variants and inverted white) follow the foreground.
    "black" or "#1C1F23" or "white" or "#110000" or "#2B2F36" or "#282C2F" => 0,
    "#A647FF" => 1,
    string s when s.StartsWith("url(", StringComparison.Ordinal) => 2,
    _ => throw new NotSupportedException($"Unmapped fill color: {fill}"),
};

static int MapMainStroke(string? stroke) => stroke switch
{
    null or "" or "none" => -1,
    "black" or "#1C1F23" => 0,
    string s when s.StartsWith("url(", StringComparison.Ordinal) => 2,
    _ => throw new NotSupportedException($"Unmapped stroke color: {stroke}"),
};

static string MapLineCap(string cap) => cap switch
{
    "butt" => "PenLineCap.Flat",
    "round" => "PenLineCap.Round",
    "square" => "PenLineCap.Square",
    _ => throw new NotSupportedException($"Unmapped stroke-linecap: {cap}"),
};

static string MapLineJoin(string join) => join switch
{
    "miter" => "PenLineJoin.Miter",
    "round" => "PenLineJoin.Round",
    "bevel" => "PenLineJoin.Bevel",
    _ => throw new NotSupportedException($"Unmapped stroke-linejoin: {join}"),
};

// --- Code emission ---

static string GeometryExpression(string d, string? fillRule)
{
    var parse = $"StreamGeometry.Parse(\"{d}\")";
    return fillRule == "evenodd"
        ? $"new GeometryGroup {{ FillRule = FillRule.EvenOdd, Children = {{ {parse} }} }}"
        : parse;
}

static string GenerateMainDocument(string className, List<SvgElement> elements, double viewBoxWidth,
    double viewBoxHeight)
{
    var sb = new StringBuilder();
    sb.AppendLine("// AutoGenerated by IRIHI Iconica Semi IconGenerator");
    sb.AppendLine("using Avalonia;");
    sb.AppendLine("using Avalonia.Media;");
    sb.AppendLine();
    sb.AppendLine("namespace Irihi.Iconica.Semi.Icons;");
    sb.AppendLine();
    sb.AppendLine($"public class {className} : SemiIconBase");
    sb.AppendLine("{");
    sb.AppendLine("    private static readonly DrawingElement[]? StaticDrawingData =");
    sb.AppendLine("    [");

    foreach (var element in elements)
    {
        var fillIndex = MapMainFill(element.Fill);
        var strokeIndex = MapMainStroke(element.Stroke);
        AppendShapeOpening(sb, element, fillIndex >= 0 ? $"FillIndex = {fillIndex}," : null,
            strokeIndex >= 0 ? $"StrokeIndex = {strokeIndex}," : null);
        if (strokeIndex >= 0)
        {
            if (element.StrokeWidth.Length > 0)
                sb.AppendLine($"            StrokeThickness = {Format(double.Parse(element.StrokeWidth, CultureInfo.InvariantCulture))},");
            if (element.LineCap.Length > 0)
                sb.AppendLine($"            LineCap = {MapLineCap(element.LineCap)},");
            if (element.LineJoin.Length > 0)
                sb.AppendLine($"            LineJoin = {MapLineJoin(element.LineJoin)},");
        }
        AppendShapeBody(sb, element);
    }

    sb.AppendLine("    ];");
    sb.AppendLine();
    sb.AppendLine("    protected override DrawingElement[]? DrawingData => StaticDrawingData;");
    if (viewBoxWidth != 24 || viewBoxHeight != 24)
    {
        sb.AppendLine();
        sb.AppendLine($"    protected override Size ViewBoxSize => new({Format(viewBoxWidth)}, {Format(viewBoxHeight)});");
    }
    sb.AppendLine("}");

    return sb.ToString();
}

static string GenerateLabDocument(string className, List<SvgElement> elements, double viewBoxWidth,
    double viewBoxHeight)
{
    var sb = new StringBuilder();
    sb.AppendLine("// AutoGenerated by IRIHI Iconica Semi IconGenerator");
    sb.AppendLine("using Avalonia;");
    sb.AppendLine("using Avalonia.Media;");
    sb.AppendLine();
    sb.AppendLine("namespace Irihi.Iconica.Semi.Lab;");
    sb.AppendLine();
    sb.AppendLine($"public class {className} : SemiLabIconBase");
    sb.AppendLine("{");
    sb.AppendLine("    private static readonly DrawingElement[]? StaticDrawingData =");
    sb.AppendLine("    [");

    foreach (var element in elements)
    {
        AppendShapeOpening(sb, element, null, null);

        var hasFill = element.Fill.Length > 0 && element.Fill != "none";
        var hasStroke = element.Stroke.Length > 0 && element.Stroke != "none";
        if (hasFill)
            sb.AppendLine($"            Fill = new SolidColorBrush(Color.Parse(\"{element.Fill}\")),");
        if (hasStroke)
        {
            var thickness = element.StrokeWidth.Length > 0
                ? Format(double.Parse(element.StrokeWidth, CultureInfo.InvariantCulture))
                : "1";
            var penArgs = $"new SolidColorBrush(Color.Parse(\"{element.Stroke}\")), {thickness}";
            if (element.LineCap.Length > 0) penArgs += $", lineCap: {MapLineCap(element.LineCap)}";
            if (element.LineJoin.Length > 0) penArgs += $", lineJoin: {MapLineJoin(element.LineJoin)}";
            sb.AppendLine($"            Pen = new Pen({penArgs}),");
        }
        if (!hasFill && !hasStroke)
            throw new NotSupportedException($"Lab element with neither fill nor stroke: {element.Kind} in {className}");

        AppendShapeBody(sb, element);
    }

    sb.AppendLine("    ];");
    sb.AppendLine();
    sb.AppendLine("    protected override DrawingElement[]? DrawingData => StaticDrawingData;");
    if (viewBoxWidth != 24 || viewBoxHeight != 24)
    {
        sb.AppendLine();
        sb.AppendLine($"    protected override Size ViewBoxSize => new({Format(viewBoxWidth)}, {Format(viewBoxHeight)});");
    }
    sb.AppendLine("}");

    return sb.ToString();
}

static void AppendShapeOpening(StringBuilder sb, SvgElement element, string? fillLine, string? strokeLine)
{
    sb.AppendLine($"        new {CharKind(element.Kind)}DrawingElement()");
    sb.AppendLine("        {");
    if (fillLine is not null) sb.AppendLine($"            {fillLine}");
    if (strokeLine is not null) sb.AppendLine($"            {strokeLine}");
}

static string CharKind(string kind) => kind switch
{
    "path" => "Path",
    "circle" or "ellipse" => "Ellipse",
    "rect" => "Rect",
    "line" => "Line",
    _ => throw new NotSupportedException($"Unknown shape: {kind}"),
};

static void AppendShapeBody(StringBuilder sb, SvgElement element)
{
    switch (element.Kind)
    {
        case "path":
            sb.AppendLine($"            Data = {GeometryExpression(element.D!, element.FillRule)},");
            break;
        case "circle":
        case "ellipse":
        {
            var rx = element.R.Length > 0 ? element.R : element.Rx;
            var ry = element.R.Length > 0 ? element.R : element.Ry;
            sb.AppendLine($"            RadiusX = {Format(Num(rx))},");
            sb.AppendLine($"            RadiusY = {Format(Num(ry))},");
            sb.AppendLine($"            X = {Format(Num(element.Cx))},");
            sb.AppendLine($"            Y = {Format(Num(element.Cy))},");
            break;
        }
        case "rect":
            sb.AppendLine($"            Width = {Format(Num(element.Width))},");
            sb.AppendLine($"            Height = {Format(Num(element.Height))},");
            sb.AppendLine($"            X = {Format(Num(element.X))},");
            sb.AppendLine($"            Y = {Format(Num(element.Y))},");
            if (element.Rx.Length > 0) sb.AppendLine($"            Rx = {Format(Num(element.Rx))},");
            if (element.Ry.Length > 0) sb.AppendLine($"            Ry = {Format(Num(element.Ry))},");
            break;
        case "line":
            sb.AppendLine($"            X1 = {Format(Num(element.X1))},");
            sb.AppendLine($"            Y1 = {Format(Num(element.Y1))},");
            sb.AppendLine($"            X2 = {Format(Num(element.X2))},");
            sb.AppendLine($"            Y2 = {Format(Num(element.Y2))},");
            break;
    }

    var matrix = CreateMatrix(element.Transform);
    if (matrix is not null) sb.AppendLine($"            Transform = {matrix},");
    sb.AppendLine("        },");
}

static string GenerateDemoDocument(List<IconInfo> icons)
{
    var sb = new StringBuilder();
    sb.AppendLine("// AutoGenerated by IRIHI Iconica Semi IconGenerator");
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("namespace Irihi.Iconica.Demo.Models;");
    sb.AppendLine("public partial class IconInfo");
    sb.AppendLine("{");
    sb.AppendLine("    public static List<IconInfo> SemiIconInfos { get; } = CreateSemiIconInfos();");
    sb.AppendLine();
    sb.AppendLine("    private static List<IconInfo> CreateSemiIconInfos()");
    sb.AppendLine("    {");
    sb.AppendLine($"        var list = new List<IconInfo>({icons.Count});");
    var chunkCount = (icons.Count + 99) / 100;
    for (var chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
        sb.AppendLine($"        list.AddRange(CreateSemiChunk_{chunkIndex:D4}());");
    sb.AppendLine("        return list;");
    sb.AppendLine("    }");
    for (var chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
    {
        sb.AppendLine();
        sb.AppendLine($"    private static List<IconInfo> CreateSemiChunk_{chunkIndex:D4}() =>");
        sb.AppendLine("    [");
        for (var i = chunkIndex * 100; i < icons.Count && i < (chunkIndex + 1) * 100; i++)
        {
            var icon = icons[i];
            sb.AppendLine("        new IconInfo()");
            sb.AppendLine("        {");
            sb.AppendLine($"            Category = \"{icon.Category}\",");
            sb.AppendLine($"            CategoryChinese = \"{icon.CategoryCN}\",");
            sb.AppendLine($"            ClassName = \"{icon.ClassName}\",");
            sb.AppendLine($"            Name = \"{icon.Name}\",");
            sb.AppendLine($"            Title = \"{icon.Title}\",");
            sb.AppendLine($"            Creator = () => new Irihi.Iconica.Semi.{icon.Namespace}.{icon.ClassName}(),");
            var keywords = icon.Keywords.Append(icon.Name).Append(icon.ClassName).ToList();
            sb.AppendLine($"            Keywords = [{string.Join(", ", keywords.Select(k => $"\"{EscapeCsString(k)}\""))}],");
            sb.AppendLine($"            IconType = typeof(Irihi.Iconica.Semi.{icon.Namespace}.{icon.ClassName})");
            sb.AppendLine("        },");
        }
        sb.AppendLine("    ];");
    }
    sb.AppendLine("}");

    return sb.ToString();
}

// --- Transform parsing: SVG function list composed into a single matrix ---

static string? CreateMatrix(string? transformString)
{
    if (string.IsNullOrEmpty(transformString)) return null;

    double[] result = [1, 0, 0, 1, 0, 0];
    foreach (Match match in Regex.Matches(transformString, @"(\w+)\s*\(([^)]*)\)"))
    {
        var name = match.Groups[1].Value;
        var args = match.Groups[2].Value
            .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();
        var func = name switch
        {
            "translate" => new[] { 1, 0, 0, 1, args[0], args.Length > 1 ? args[1] : 0 },
            "scale" => new[] { args[0], 0, 0, args.Length > 1 ? args[1] : args[0], 0, 0 },
            "rotate" => args.Length == 1
                ? RotateMatrix(args[0], 0, 0)
                : RotateMatrix(args[0], args[1], args[2]),
            "matrix" when args.Length == 6 => args,
            _ => throw new NotSupportedException($"Unsupported transform function: {name}"),
        };
        result = Multiply(result, func);
    }

    if (result is [1, 0, 0, 1, 0, 0]) return null;
    return $"new Matrix({string.Join(", ", result.Select(Format))})";
}

static double[] RotateMatrix(double angleDegrees, double cx, double cy)
{
    var radians = angleDegrees * (Math.PI / 180.0);
    var cos = Math.Cos(radians);
    var sin = Math.Sin(radians);
    // translate(cx, cy) * rotate(a) * translate(-cx, -cy), column convention.
    return
    [
        cos, sin, -sin, cos,
        cx * (1 - cos) + cy * sin,
        cy * (1 - cos) - cx * sin,
    ];
}

// Column-convention matrix product: (m1 * m2) applies m2 first, then m1 — matching SVG lists.
static double[] Multiply(double[] m1, double[] m2) =>
[
    m1[0] * m2[0] + m1[2] * m2[1],
    m1[1] * m2[0] + m1[3] * m2[1],
    m1[0] * m2[2] + m1[2] * m2[3],
    m1[1] * m2[2] + m1[3] * m2[3],
    m1[0] * m2[4] + m1[2] * m2[5] + m1[4],
    m1[1] * m2[4] + m1[3] * m2[5] + m1[5],
];

// --- Misc helpers ---

static (double Width, double Height) ParseViewBox(string viewBox)
{
    if (viewBox.Length == 0) return (24, 24);
    var parts = viewBox.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 4) throw new NotSupportedException($"Unexpected viewBox: {viewBox}");
    return (double.Parse(parts[2], CultureInfo.InvariantCulture),
        double.Parse(parts[3], CultureInfo.InvariantCulture));
}

static double Num(string value)
    => value.Length > 0 ? double.Parse(value, CultureInfo.InvariantCulture) : 0;

static string ToPascalCase(string name)
{
    var parts = name.Split('_', '-');
    var sb = new StringBuilder();
    foreach (var part in parts)
    {
        if (part.Length == 0) continue;
        // Official components keep the AI acronym uppercase (IconAIBellLevel1).
        if (part == "ai") { sb.Append("AI"); continue; }
        sb.Append(char.ToUpperInvariant(part[0])).Append(part.AsSpan(1));
    }
    if (sb.Length == 0) throw new NotSupportedException($"Empty name: {name}");
    if (char.IsDigit(sb[0])) sb.Insert(0, '_');
    return sb.ToString();
}

static string SplitCamelCase(string name) => Regex.Replace(name, "(?<=[a-z0-9])([A-Z])", " $1");

static string Format(double value) => value.ToString("0.########", CultureInfo.InvariantCulture);

static string EscapeCsString(string value)
    => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

static Dictionary<string, string> LoadCategories(string path)
{
    var result = new Dictionary<string, string>(StringComparer.Ordinal);
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    foreach (var entry in doc.RootElement.EnumerateArray())
    {
        var name = entry.GetProperty("name").GetString()!;
        // meta.json names AI icons "a_i_bell_level_1" while the SVG files use "ai_bell_level_1".
        if (name.StartsWith("a_i_", StringComparison.Ordinal)) name = "ai_" + name["a_i_".Length..];
        result[name] = entry.GetProperty("category").GetString() ?? string.Empty;
    }

    return result;
}

static Dictionary<string, string> CreateCategoryChinese() => new()
{
    ["Basic"] = "基础",
    ["Interface"] = "界面",
    ["Objects"] = "物品",
    ["Magic Customized"] = "魔法定制",
    ["Writing"] = "写作",
    ["Arrow"] = "箭头",
    ["Brands"] = "品牌",
    ["Music"] = "音乐",
    ["Communication"] = "沟通",
    ["Business"] = "商业",
    ["Photo&Video"] = "图像与视频",
    ["Editor"] = "编辑器",
    ["Chart & Diagram"] = "图表与图示",
    ["Layout"] = "布局",
    ["User"] = "用户",
    ["Device"] = "设备",
    ["Time"] = "时间",
    ["Files&Folder"] = "文件与文件夹",
    ["Accessibility"] = "无障碍",
    ["Code"] = "代码",
    ["Alert"] = "警示",
    ["Building"] = "建筑",
    ["One Color"] = "单色",
    ["Two Color"] = "双色",
    ["Multiple Color"] = "多色",
};

// Intermediate SVG shape representation shared by both emitters.
class SvgElement
{
    public string Kind { get; set; } = string.Empty;
    public string D { get; set; } = string.Empty;
    public string Cx { get; set; } = string.Empty;
    public string Cy { get; set; } = string.Empty;
    public string R { get; set; } = string.Empty;
    public string Rx { get; set; } = string.Empty;
    public string Ry { get; set; } = string.Empty;
    public string X { get; set; } = string.Empty;
    public string Y { get; set; } = string.Empty;
    public string Width { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string X1 { get; set; } = string.Empty;
    public string Y1 { get; set; } = string.Empty;
    public string X2 { get; set; } = string.Empty;
    public string Y2 { get; set; } = string.Empty;
    public string Fill { get; set; } = string.Empty;
    public string Stroke { get; set; } = string.Empty;
    public string StrokeWidth { get; set; } = string.Empty;
    public string LineCap { get; set; } = string.Empty;
    public string LineJoin { get; set; } = string.Empty;
    public string FillRule { get; set; } = string.Empty;
    public string Transform { get; set; } = string.Empty;
}
