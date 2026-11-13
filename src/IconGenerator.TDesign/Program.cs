using System.Globalization;
using System.Text;
using System.Xml;
using IconGenerator.TDesign;
using Jint;

// Repository root: explicit --root <path> wins; otherwise infer from build output location
// (src/IconGenerator.TDesign/bin/{Config}/{TFM} -> 5 levels up).
var rootPath = args.Length >= 2 && args[0] == "--root"
    ? Path.GetFullPath(args[1])
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var svgPath = Path.Combine(rootPath, "external", "tdesign-icons", "svg");
var svgFiles = Directory.GetFiles(svgPath, "*.svg").OrderBy(f => f, StringComparer.Ordinal).ToArray();

var sourceTargetPath = Path.Combine(rootPath, "src", "Irihi.Iconica.TDesign", "Generated");
var outlineTargetPath = Path.Combine(sourceTargetPath, "Outline");
var filledTargetPath = Path.Combine(sourceTargetPath, "Filled");
if (Directory.Exists(sourceTargetPath)) Directory.Delete(sourceTargetPath, true);
Directory.CreateDirectory(outlineTargetPath);
Directory.CreateDirectory(filledTargetPath);

var manifestPath = Path.Combine(rootPath, "external", "tdesign-icons", "packages", "view", "src", "manifest.js");
var manifest = LoadManifest(manifestPath);

var iconsInfo = new List<IconInfo>();
var failures = new List<(string File, string Error)>();

foreach (var svgFile in svgFiles)
{
    try
    {
        var fileName = Path.GetFileName(svgFile);
        var isFilledFile = fileName.EndsWith("-filled.svg", StringComparison.Ordinal);
        var baseName = fileName[..(fileName.Length - ".svg".Length)];
        if (isFilledFile) baseName = baseName[..(baseName.Length - "-filled".Length)];
        var className = ToPascalCase(baseName);
        if (isFilledFile) className += "Filled";
        var iconName = isFilledFile ? baseName + "-filled" : baseName;

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(svgFile);

        var elements = new List<DrawingElement>();
        var root = xmlDoc.DocumentElement!;
        WalkElements(root, null, elements);

        var hasStroke = elements.Any(e => e.StrokeIndex >= 0);
        var targetDir = hasStroke ? outlineTargetPath : filledTargetPath;

        var sourceCode = GenerateDocument(className, elements);
        var targetPath = Path.Combine(targetDir, $"{className}.cs");
        await File.WriteAllTextAsync(targetPath, sourceCode);

        var meta = manifest.GetValueOrDefault(iconName);
        iconsInfo.Add(new IconInfo
        {
            ClassName = className,
            Name = iconName,
            Category = meta?.Category ?? string.Empty,
            CategoryCN = meta?.CategoryCN ?? string.Empty,
            Title = meta is { Keywords.Count: > 0 } ? meta.Keywords[0] : string.Empty,
            Keywords = meta?.Keywords ?? [],
        });
    }
    catch (Exception e)
    {
        failures.Add((svgFile, e.Message));
        Console.Error.WriteLine($"Failed: {svgFile}");
        Console.Error.WriteLine(e.Message);
    }
}

var demoFile = Path.Combine(rootPath, "demo", "Irihi.Iconica.Demo", "Models", "IconInfo.TDesign.Generated.cs");
var demoCode = GenerateDemoDocument(iconsInfo);
await File.WriteAllTextAsync(demoFile, demoCode);

Console.WriteLine($"Generated {iconsInfo.Count} icons -> {sourceTargetPath}");
Console.WriteLine($"Generated demo info -> {demoFile}");
if (failures.Count > 0)
{
    Console.Error.WriteLine($"{failures.Count} icons failed to generate:");
    foreach (var (file, error) in failures)
        Console.Error.WriteLine($"  {Path.GetFileName(file)}: {error}");
    Environment.Exit(1);
}

void WalkElements(XmlElement parent, string? inheritedLayer, List<DrawingElement> result)
{
    foreach (var node in parent.ChildNodes)
    {
        if (node is not XmlElement element) continue;

        var layer = inheritedLayer;
        if (element.Name == "g")
        {
            var id = element.GetAttribute("id");
            if (IsLayerId(id)) layer = id;
            WalkElements(element, layer, result);
            continue;
        }

        if (element.Name is not ("path" or "ellipse" or "rect" or "circle" or "line")) continue;

        var selfId = element.GetAttribute("id");
        var effectiveLayer = IsLayerId(selfId) ? selfId : layer;

        var fill = element.GetAttribute("fill");
        var stroke = element.GetAttribute("stroke");
        var fillIndex = -1;
        var strokeIndex = -1;
        if (TryGetLayerNumber(effectiveLayer, "fill", out var fillN)) fillIndex = fillN + 1;
        else if (fill.Length > 0 && fill != "none") fillIndex = 0;
        if (TryGetLayerNumber(effectiveLayer, "stroke", out var strokeN)) strokeIndex = strokeN - 1;
        else if (stroke.Length > 0) strokeIndex = 0;

        var transform = element.GetAttribute("transform");
        var matrix = CreateMatrix(transform);

        switch (element.Name)
        {
            case "path":
            {
                var d = element.GetAttribute("d");
                if (d.Length == 0) continue;
                result.Add(new PathDrawingElement
                {
                    Data = d,
                    FillIndex = fillIndex,
                    StrokeIndex = strokeIndex,
                    Transform = matrix,
                });
                break;
            }
            case "circle":
            case "ellipse":
            {
                var cx = ParseDouble(element.GetAttribute("cx"));
                var cy = ParseDouble(element.GetAttribute("cy"));
                var r = ParseDouble(element.GetAttribute("r"));
                var rx = r ?? ParseDouble(element.GetAttribute("rx")) ?? 0;
                var ry = r ?? ParseDouble(element.GetAttribute("ry")) ?? 0;
                result.Add(new EllipseDrawingElement
                {
                    X = cx ?? 0,
                    Y = cy ?? 0,
                    RadiusX = rx,
                    RadiusY = ry,
                    FillIndex = fillIndex,
                    StrokeIndex = strokeIndex,
                    Transform = matrix,
                });
                break;
            }
            case "rect":
            {
                result.Add(new RectDrawingElement
                {
                    X = ParseDouble(element.GetAttribute("x")) ?? 0,
                    Y = ParseDouble(element.GetAttribute("y")) ?? 0,
                    Width = ParseDouble(element.GetAttribute("width")) ?? 0,
                    Height = ParseDouble(element.GetAttribute("height")) ?? 0,
                    Rx = ParseDouble(element.GetAttribute("rx")),
                    Ry = ParseDouble(element.GetAttribute("ry")),
                    FillIndex = fillIndex,
                    StrokeIndex = strokeIndex,
                    Transform = matrix,
                });
                break;
            }
            case "line":
            {
                result.Add(new LineDrawingElement
                {
                    X1 = ParseDouble(element.GetAttribute("x1")) ?? 0,
                    Y1 = ParseDouble(element.GetAttribute("y1")) ?? 0,
                    X2 = ParseDouble(element.GetAttribute("x2")) ?? 0,
                    Y2 = ParseDouble(element.GetAttribute("y2")) ?? 0,
                    FillIndex = fillIndex,
                    StrokeIndex = strokeIndex,
                    Transform = matrix,
                });
                break;
            }
        }
    }
}

static bool IsLayerId(string id) => id.StartsWith("fill") || id.StartsWith("stroke");

static bool TryGetLayerNumber(string? layerId, string prefix, out int number)
{
    number = 0;
    if (string.IsNullOrEmpty(layerId) || !layerId.StartsWith(prefix, StringComparison.Ordinal)) return false;
    return int.TryParse(layerId.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out number);
}

static double? ParseDouble(string value)
    => value.Length > 0 ? double.Parse(value, CultureInfo.InvariantCulture) : null;

static string ToPascalCase(string name)
{
    var parts = name.Split('-', StringSplitOptions.RemoveEmptyEntries);
    var sb = new StringBuilder();
    foreach (var part in parts)
        sb.Append(char.ToUpperInvariant(part[0])).Append(part.AsSpan(1));
    if (char.IsDigit(sb[0])) sb.Insert(0, '_');
    return sb.ToString();
}

static string GenerateDocument(string className, List<DrawingElement> elements)
{
    var sb = new StringBuilder();
    sb.AppendLine("// AutoGenerated by IRIHI Iconica TDesign IconGenerator");
    sb.AppendLine("using Avalonia;");
    sb.AppendLine("using Avalonia.Media;");
    sb.AppendLine();
    sb.AppendLine("namespace Irihi.Iconica.TDesign.Icons;");
    sb.AppendLine();
    sb.AppendLine($"public class {className} : TDesignIconBase");
    sb.AppendLine("{");
    sb.AppendLine("    private static readonly DrawingElement[]? StaticDrawingData =");
    sb.AppendLine("    [");

    foreach (var element in elements)
    {
        if (element is PathDrawingElement pathData)
        {
            sb.AppendLine("        new PathDrawingElement()");
            sb.AppendLine("        {");
            if (pathData.FillIndex >= 0) sb.AppendLine($"            FillIndex = {pathData.FillIndex},");
            if (pathData.StrokeIndex >= 0) sb.AppendLine($"            StrokeIndex = {pathData.StrokeIndex},");
            sb.AppendLine("            Data = StreamGeometry.Parse(");
            sb.AppendLine($"                \"{pathData.Data}\"),");
            if (pathData.Transform is not null) sb.AppendLine($"            Transform = {pathData.Transform},");
            sb.AppendLine("        },");
        }
        else if (element is EllipseDrawingElement ellipseData)
        {
            sb.AppendLine("        new EllipseDrawingElement()");
            sb.AppendLine("        {");
            sb.AppendLine($"            RadiusX = {Format(ellipseData.RadiusX)},");
            sb.AppendLine($"            RadiusY = {Format(ellipseData.RadiusY)},");
            sb.AppendLine($"            X = {Format(ellipseData.X)},");
            sb.AppendLine($"            Y = {Format(ellipseData.Y)},");
            if (ellipseData.FillIndex >= 0) sb.AppendLine($"            FillIndex = {ellipseData.FillIndex},");
            if (ellipseData.StrokeIndex >= 0) sb.AppendLine($"            StrokeIndex = {ellipseData.StrokeIndex},");
            if (ellipseData.Transform is not null) sb.AppendLine($"            Transform = {ellipseData.Transform},");
            sb.AppendLine("        },");
        }
        else if (element is RectDrawingElement rectData)
        {
            sb.AppendLine("        new RectDrawingElement()");
            sb.AppendLine("        {");
            sb.AppendLine($"            Width = {Format(rectData.Width)},");
            sb.AppendLine($"            Height = {Format(rectData.Height)},");
            sb.AppendLine($"            X = {Format(rectData.X)},");
            sb.AppendLine($"            Y = {Format(rectData.Y)},");
            if (rectData.Rx is not null) sb.AppendLine($"            Rx = {Format(rectData.Rx.Value)},");
            if (rectData.Ry is not null) sb.AppendLine($"            Ry = {Format(rectData.Ry.Value)},");
            if (rectData.FillIndex >= 0) sb.AppendLine($"            FillIndex = {rectData.FillIndex},");
            if (rectData.StrokeIndex >= 0) sb.AppendLine($"            StrokeIndex = {rectData.StrokeIndex},");
            if (rectData.Transform is not null) sb.AppendLine($"            Transform = {rectData.Transform},");
            sb.AppendLine("        },");
        }
        else if (element is LineDrawingElement lineData)
        {
            sb.AppendLine("        new LineDrawingElement()");
            sb.AppendLine("        {");
            sb.AppendLine($"            X1 = {Format(lineData.X1)},");
            sb.AppendLine($"            Y1 = {Format(lineData.Y1)},");
            sb.AppendLine($"            X2 = {Format(lineData.X2)},");
            sb.AppendLine($"            Y2 = {Format(lineData.Y2)},");
            if (lineData.FillIndex >= 0) sb.AppendLine($"            FillIndex = {lineData.FillIndex},");
            if (lineData.StrokeIndex >= 0) sb.AppendLine($"            StrokeIndex = {lineData.StrokeIndex},");
            if (lineData.Transform is not null) sb.AppendLine($"            Transform = {lineData.Transform},");
            sb.AppendLine("        },");
        }
    }

    sb.AppendLine("    ];");
    sb.AppendLine();
    sb.AppendLine("    protected override DrawingElement[]? DrawingData => StaticDrawingData;");
    sb.AppendLine("}");

    return sb.ToString();
}

static string GenerateDemoDocument(List<IconInfo> icons)
{
    var sb = new StringBuilder();
    sb.AppendLine("// AutoGenerated by IRIHI Iconica TDesign IconGenerator");
    sb.AppendLine("using System;");
    sb.AppendLine("using System.Collections.Generic;");
    sb.AppendLine("using Irihi.Iconica.TDesign.Icons;");
    sb.AppendLine("namespace Irihi.Iconica.Demo.Models;");
    sb.AppendLine("public partial class IconInfo");
    sb.AppendLine("{");
    sb.AppendLine("    public static List<IconInfo> TDesignIconInfos { get; } = CreateTDesignIconInfos();");
    sb.AppendLine();
    sb.AppendLine("    private static List<IconInfo> CreateTDesignIconInfos()");
    sb.AppendLine("    {");
    sb.AppendLine($"        var list = new List<IconInfo>({icons.Count});");
    var tDesignChunkCount = (icons.Count + 99) / 100;
    for (var chunkIndex = 0; chunkIndex < tDesignChunkCount; chunkIndex++)
        sb.AppendLine($"        list.AddRange(CreateTDesignChunk_{chunkIndex:D4}());");
    sb.AppendLine("        return list;");
    sb.AppendLine("    }");
    for (var chunkIndex = 0; chunkIndex < tDesignChunkCount; chunkIndex++)
    {
        sb.AppendLine();
        sb.AppendLine($"    private static List<IconInfo> CreateTDesignChunk_{chunkIndex:D4}() =>");
        sb.AppendLine("    [");
        for (var i = chunkIndex * 100; i < icons.Count && i < (chunkIndex + 1) * 100; i++)
        {
            var icon = icons[i];
            sb.AppendLine("        new IconInfo()");
            sb.AppendLine("        {");
            if (icon.Category.Length > 0) sb.AppendLine($"            Category = \"{icon.Category}\",");
            if (icon.CategoryCN.Length > 0) sb.AppendLine($"            CategoryChinese = \"{icon.CategoryCN}\",");
            sb.AppendLine($"            ClassName = \"{icon.ClassName}\",");
            sb.AppendLine($"            Name = \"{icon.Name}\",");
            if (icon.Title.Length > 0) sb.AppendLine($"            Title = \"{icon.Title}\",");
            if (icon.Keywords.Count > 0)
                sb.AppendLine($"            Tag = [{string.Join(", ", icon.Keywords.Select(k => $"\"{EscapeCsString(k)}\""))}],");
            sb.AppendLine($"            Creator = () => new Irihi.Iconica.TDesign.Icons.{icon.ClassName}(),");
            var keywords = icon.Keywords.Concat([icon.Name, icon.ClassName]).ToList();
            sb.AppendLine($"            Keywords = [{string.Join(", ", keywords.Select(k => $"\"{EscapeCsString(k)}\""))}],");
            sb.AppendLine($"            IconType = typeof(Irihi.Iconica.TDesign.Icons.{icon.ClassName})");
            sb.AppendLine("        },");
        }
        sb.AppendLine("    ];");
    }
    sb.AppendLine("}");

    return sb.ToString();
}

static string EscapeCsString(string value)
    => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

static Dictionary<string, IconMeta> LoadManifest(string path)
{
    var result = new Dictionary<string, IconMeta>();
    if (!File.Exists(path))
    {
        Console.WriteLine($"Manifest not found: {path}");
        return result;
    }

    try
    {
        var js = File.ReadAllText(path);
        var marker = "export const manifest =";
        var index = js.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0)
        {
            Console.WriteLine($"Unexpected manifest format: {path}");
            return result;
        }

        // manifest.js 是 ES 模块(export const),Jint 是脚本执行器,const 声明不进全局对象,
        // 去掉 export 并改用 var 后执行,再从全局对象取值。
        js = js[..index] + "var manifest =" + js[(index + marker.Length)..];
        var engine = new Engine();
        engine.Execute(js);
        if (engine.GetValue("manifest").ToObject() is not IDictionary<string, object> root)
        {
            Console.WriteLine($"Failed to parse manifest: {path}");
            return result;
        }

        foreach (var typeValue in root.Values)
        {
            if (typeValue is not IDictionary<string, object> categories) continue;
            foreach (var categoryValue in categories.Values)
            {
                if (categoryValue is not IDictionary<string, object> category) continue;
                var labelCN = GetValue<string>(category, "labelCN") ?? string.Empty;
                var labelEn = GetValue<string>(category, "labelEn") ?? string.Empty;
                if (GetValue<object[]>(category, "icons") is not { } icons) continue;
                foreach (var iconValue in icons)
                {
                    if (iconValue is not IDictionary<string, object> icon) continue;
                    if (GetValue<string>(icon, "name") is not { } name) continue;
                    var keywords = new List<string>();
                    if (GetValue<object[]>(icon, "keywords") is { } keywordList)
                        keywords.AddRange(keywordList.OfType<string>());
                    result[name] = new IconMeta(labelEn, labelCN, keywords);
                }
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"Failed to parse manifest: {e.Message}");
    }

    return result;
}

static T? GetValue<T>(IDictionary<string, object> dict, string key)
    => dict.TryGetValue(key, out var value) && value is T typed ? typed : default;

static string? CreateMatrix(string? transformString)
{
    if (string.IsNullOrEmpty(transformString)) return null;
    if (transformString.StartsWith("matrix"))
    {
        var values = transformString.Split('(')[1].Split(')')[0]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matrixValues = string.Join(", ",
            values.Select(v => Format(double.Parse(v, CultureInfo.InvariantCulture))));
        return $"new Matrix({matrixValues})";
    }

    if (transformString.StartsWith("rotate"))
    {
        var values = transformString.Split('(')[1].Split(')')[0]
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (values.Length == 1)
            return $"Matrix.CreateRotation({Format(double.Parse(values[0], CultureInfo.InvariantCulture))})";
        var angle = double.Parse(values[0], CultureInfo.InvariantCulture);
        var x = double.Parse(values[1], CultureInfo.InvariantCulture);
        var y = double.Parse(values[2], CultureInfo.InvariantCulture);
        var radians = angle * (Math.PI / 180.0);
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return $"new Matrix({Format(cos)}, {Format(sin)}, {Format(-sin)}, {Format(cos)}, " +
               $"{Format(x * (1.0 - cos) + y * sin)}, {Format(y * (1.0 - cos) - x * sin)})";
    }

    return null;
}

static string Format(double value) => value.ToString("0.########", CultureInfo.InvariantCulture);
