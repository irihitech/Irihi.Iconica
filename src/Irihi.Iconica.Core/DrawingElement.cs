using Avalonia;
using Avalonia.Media;

namespace Irihi.Iconica.Core;

public abstract class DrawingElement
{
    public int StrokeIndex { get; set; } = -1;
    public int FillIndex { get; set; } = -1;
    public bool InheritStrokeWidth { get; set; }
    public bool InheritStrokeCap { get; set; }
    public bool InheritStrokeJoin { get; set; }
    public Matrix? Transform { get; set; }

    /// <summary>
    /// Direct fill brush for elements with fixed colors. When set, it takes precedence
    /// over the slot brush resolved from <see cref="FillIndex"/>.
    /// </summary>
    public IBrush? Fill { get; set; }

    /// <summary>
    /// Direct pen for elements with fully fixed strokes (fixed-color icon sets).
    /// When set, it takes precedence over the slot pen resolved from <see cref="StrokeIndex"/>.
    /// </summary>
    public IPen? Pen { get; set; }

    /// <summary>Per-element stroke thickness override applied to the resolved pen.</summary>
    public double? StrokeThickness { get; set; }

    /// <summary>Per-element line cap override applied to the resolved pen.</summary>
    public PenLineCap? LineCap { get; set; }

    /// <summary>Per-element line join override applied to the resolved pen.</summary>
    public PenLineJoin? LineJoin { get; set; }
}

public class PathDrawingElement : DrawingElement
{
    public Geometry? Data { get; set; }
}

public class EllipseDrawingElement : DrawingElement
{
    public double RadiusX { get; set; }
    public double RadiusY { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

public class LineDrawingElement : DrawingElement
{
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
}

public class RectDrawingElement : DrawingElement
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double? Rx { get; set; }
    public double? Ry { get; set; }
}