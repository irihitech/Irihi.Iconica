using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Irihi.Iconica.Semi.Lab;

/// <summary>
/// Base class for the Semi Design "lab" icon set: colorful illustrative icons whose
/// palette is part of the design. Colors are baked into each generated element, so
/// this base only handles size and the (occasionally non-24) source viewBox.
/// </summary>
public abstract class SemiLabIconBase : Control
{
    static SemiLabIconBase()
    {
        WidthProperty.OverrideDefaultValue<SemiLabIconBase>(24);
        HeightProperty.OverrideDefaultValue<SemiLabIconBase>(24);
    }

    protected SemiLabIconBase()
    {
    }

    protected abstract DrawingElement[]? DrawingData { get; }

    /// <summary>Source viewBox of the icon; overridden by generated classes when it is not 24x24.</summary>
    protected virtual Size ViewBoxSize => new(24, 24);

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (DrawingData is null) return;

        // Uniform "meet" scaling with centering, honoring the source viewBox.
        var viewBox = ViewBoxSize;
        var scale = Math.Min(Bounds.Width / viewBox.Width, Bounds.Height / viewBox.Height);
        var offsetX = (Bounds.Width - viewBox.Width * scale) / 2;
        var offsetY = (Bounds.Height - viewBox.Height * scale) / 2;
        using (context.PushTransform(Matrix.CreateTranslation(offsetX, offsetY)))
        using (context.PushTransform(Matrix.CreateScale(scale, scale)))
        {
            foreach (var element in DrawingData)
            {
                switch (element)
                {
                    case PathDrawingElement pathElement:
                        context.DrawPathElement(pathElement, null, null);
                        break;
                    case EllipseDrawingElement ellipseElement:
                        context.DrawEllipseElement(ellipseElement, null, null);
                        break;
                    case LineDrawingElement lineElement:
                        var pen = element.ResolvePen(null);
                        if (pen is not null) context.DrawLineElement(lineElement, pen);
                        break;
                    case RectDrawingElement rectElement:
                        context.DrawRectElement(rectElement, null, null);
                        break;
                }
            }
        }
    }
}
