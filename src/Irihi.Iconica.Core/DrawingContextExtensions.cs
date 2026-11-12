using Avalonia;
using Avalonia.Media;

namespace Irihi.Iconica.Core;

public static class DrawingContextExtensions
{
    /// <summary>
    /// Resolves the effective brush for an element: a direct <see cref="DrawingElement.Fill"/>
    /// (fixed color) wins over the slot brush passed by the icon control.
    /// </summary>
    public static IBrush? ResolveFill(this DrawingElement element, IBrush? brush)
        => element.Fill ?? brush;

    /// <summary>
    /// Resolves the effective pen for an element: a direct <see cref="DrawingElement.Pen"/>
    /// (fixed stroke) wins over the slot pen, and per-element thickness/cap/join overrides
    /// are applied on top of the resolved pen.
    /// </summary>
    public static IPen? ResolvePen(this DrawingElement element, IPen? pen)
    {
        pen = element.Pen ?? pen;
        if (pen is not null &&
            (element.StrokeThickness is not null || element.LineCap is not null || element.LineJoin is not null))
        {
            pen = new Pen(pen.Brush, element.StrokeThickness ?? pen.Thickness,
                lineCap: element.LineCap ?? pen.LineCap,
                lineJoin: element.LineJoin ?? pen.LineJoin);
        }

        return pen;
    }

    public static void DrawPathElement(this DrawingContext context, PathDrawingElement element, IBrush? brush, IPen? pen)
    {
        if (element.Data is null) return;

        brush = element.ResolveFill(brush);
        pen = element.ResolvePen(pen);

        if (element.Transform is not null)
        {
            var transform = element.Transform.Value;
            using (context.PushTransform(transform))
                context.DrawGeometry(brush, pen, element.Data);
        }
        else
        {
            context.DrawGeometry(brush, pen, element.Data);
        }
    }

    public static void DrawEllipseElement(this DrawingContext context, EllipseDrawingElement element, IBrush? brush,
        IPen? pen)
    {
        brush = element.ResolveFill(brush);
        pen = element.ResolvePen(pen);

        if (element.Transform is not null)
        {
            var transform = element.Transform.Value;
            using (context.PushTransform(transform))
            {
                context.DrawEllipse(brush, pen, new Point(element.X, element.Y),
                    element.RadiusX, element.RadiusY);
            }
        }

        else
        {
            context.DrawEllipse(brush, pen, new Point(element.X, element.Y),
                element.RadiusX, element.RadiusY);
        }
    }

    public static void DrawLineElement(this DrawingContext context, LineDrawingElement element, IPen pen)
    {
        pen = (element.ResolvePen(pen))!;

        if (element.Transform is not null)
        {
            var transform = element.Transform.Value;
            using (context.PushTransform(transform))
            {
                context.DrawLine(pen, new Point(element.X1, element.Y1),
                    new Point(element.X2, element.Y2));
            }
        }
        else
        {
            context.DrawLine(pen, new Point(element.X1, element.Y1),
                new Point(element.X2, element.Y2));
        }
    }

    public static void DrawRectElement(this DrawingContext context, RectDrawingElement element, IBrush? brush,
        IPen? pen)
    {
        brush = element.ResolveFill(brush);
        pen = element.ResolvePen(pen);
        var rect = new Rect(element.X, element.Y, element.Width, element.Height);
        if (element.Transform is not null)
        {
            var transform = element.Transform.Value;
            using (context.PushTransform(transform))
                context.DrawRectangle(brush, pen, rect, element.Rx ?? 0, element.Ry ?? 0);
        }
        else
        {
            context.DrawRectangle(brush, pen, rect, element.Rx ?? 0, element.Ry ?? 0);
        }
    }
}
