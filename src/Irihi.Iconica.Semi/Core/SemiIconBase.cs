using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Irihi.Iconica.Semi;

/// <summary>
/// Base class for the main Semi Design icon set. Icons are single-theme-color by
/// design; two-tone (AI level 2) and gradient (AI level 3) icons resolve their
/// additional layers through the <see cref="Secondary"/> and <see cref="Gradient"/> slots.
/// </summary>
public abstract class SemiIconBase : Control
{
    public static readonly StyledProperty<IBrush?> PrimaryProperty =
        TextElement.ForegroundProperty.AddOwner<SemiIconBase>();

    public static readonly StyledProperty<IBrush?> SecondaryProperty =
        AvaloniaProperty.Register<SemiIconBase, IBrush?>(
            nameof(Secondary), new SolidColorBrush(Color.Parse("#A647FF")));

    public static readonly StyledProperty<IBrush?> GradientProperty =
        AvaloniaProperty.Register<SemiIconBase, IBrush?>(
            nameof(Gradient), CreateDefaultGradientBrush());

    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<SemiIconBase, double>(
            nameof(StrokeWidth), 2);

    public static readonly StyledProperty<PenLineCap> LineCapProperty =
        AvaloniaProperty.Register<SemiIconBase, PenLineCap>(
            nameof(LineCap), PenLineCap.Round);

    public static readonly StyledProperty<PenLineJoin> LineJoinProperty =
        AvaloniaProperty.Register<SemiIconBase, PenLineJoin>(
            nameof(LineJoin), PenLineJoin.Round);

    // 0: Primary, 1: Secondary, 2: Gradient
    private readonly IBrush?[] _brushes = new IBrush?[3];
    private readonly Pen?[] _pens = new Pen?[3];

    static SemiIconBase()
    {
        WidthProperty.OverrideDefaultValue<SemiIconBase>(24);
        HeightProperty.OverrideDefaultValue<SemiIconBase>(24);
        PrimaryProperty.Changed.AddClassHandler<SemiIconBase, IBrush?>((icon, e) => icon.InvalidateBrushes(e, 0));
        SecondaryProperty.Changed.AddClassHandler<SemiIconBase, IBrush?>((icon, e) => icon.InvalidateBrushes(e, 1));
        GradientProperty.Changed.AddClassHandler<SemiIconBase, IBrush?>((icon, e) => icon.InvalidateBrushes(e, 2));
        StrokeWidthProperty.Changed.AddClassHandler<SemiIconBase, double>((icon, e) => icon.InvalidateStrokeWidth(e));
        LineCapProperty.Changed.AddClassHandler<SemiIconBase, PenLineCap>((icon, e) => icon.InvalidateLineCap(e));
        LineJoinProperty.Changed.AddClassHandler<SemiIconBase, PenLineJoin>((icon, e) => icon.InvalidateLineJoin(e));
    }

    protected SemiIconBase()
    {
    }

    /// <summary>Main theme color; defaults to the inherited text foreground.</summary>
    public IBrush? Primary
    {
        get => GetValue(PrimaryProperty);
        set => SetValue(PrimaryProperty, value);
    }

    /// <summary>Accent color for two-tone (AI level 2) icons.</summary>
    public IBrush? Secondary
    {
        get => GetValue(SecondaryProperty);
        set => SetValue(SecondaryProperty, value);
    }

    /// <summary>Brush for gradient (AI level 3) icons; defaults to the official Semi gradient.</summary>
    public IBrush? Gradient
    {
        get => GetValue(GradientProperty);
        set => SetValue(GradientProperty, value);
    }

    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public PenLineCap LineCap
    {
        get => GetValue(LineCapProperty);
        set => SetValue(LineCapProperty, value);
    }

    public PenLineJoin LineJoin
    {
        get => GetValue(LineJoinProperty);
        set => SetValue(LineJoinProperty, value);
    }

    protected abstract DrawingElement[]? DrawingData { get; }

    /// <summary>Source viewBox of the icon; overridden by generated classes when it is not 24x24.</summary>
    protected virtual Size ViewBoxSize => new(24, 24);

    private static IBrush CreateDefaultGradientBrush()
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.917, 0.917, RelativeUnit.Relative),
            EndPoint = new RelativePoint(-0.02, 0.785, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.Parse("#E945FF"), 0),
                new GradientStop(Color.Parse("#A647FF"), 0.3),
                new GradientStop(Color.Parse("#6B61FF"), 0.6),
                new GradientStop(Color.Parse("#2E8CFF"), 1),
            },
        };
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _brushes[0] = Primary;
        _brushes[1] = Secondary;
        _brushes[2] = Gradient;
        for (var i = 0; i < 3; i++)
            _pens[i] = new Pen(_brushes[i], StrokeWidth, lineCap: LineCap, lineJoin: LineJoin);
    }

    private void InvalidateBrushes(AvaloniaPropertyChangedEventArgs<IBrush?> args, int index)
    {
        _brushes[index] = args.NewValue.Value;
        _pens[index] = new Pen(_brushes[index], StrokeWidth, lineCap: LineCap, lineJoin: LineJoin);
        InvalidateVisual();
    }

    private void InvalidateStrokeWidth(AvaloniaPropertyChangedEventArgs<double> args)
    {
        foreach (var pen in _pens)
        {
            pen?.Thickness = args.NewValue.Value;
        }

        InvalidateVisual();
    }

    private void InvalidateLineCap(AvaloniaPropertyChangedEventArgs<PenLineCap> args)
    {
        foreach (var pen in _pens)
        {
            pen?.LineCap = args.NewValue.Value;
        }

        InvalidateVisual();
    }

    private void InvalidateLineJoin(AvaloniaPropertyChangedEventArgs<PenLineJoin> args)
    {
        foreach (var pen in _pens)
        {
            pen?.LineJoin = args.NewValue.Value;
        }

        InvalidateVisual();
    }

    private IBrush? GetBrush(int index) => index is >= 0 and < 3 ? _brushes[index] : null;

    private Pen? GetPen(int index) => index is >= 0 and < 3 ? _pens[index] : null;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (DrawingData is null) return;

        // Uniform "meet" scaling: icons stay square regardless of non-square bounds.
        var viewBox = ViewBoxSize;
        var scale = Math.Min(Bounds.Width / viewBox.Width, Bounds.Height / viewBox.Height);
        var offsetX = (Bounds.Width - viewBox.Width * scale) / 2;
        var offsetY = (Bounds.Height - viewBox.Height * scale) / 2;
        using (context.PushTransform(Matrix.CreateTranslation(offsetX, offsetY)))
        using (context.PushTransform(Matrix.CreateScale(scale, scale)))
        {
            foreach (var element in DrawingData)
            {
                var brush = element.ResolveFill(GetBrush(element.FillIndex));
                var pen = element.ResolvePen(GetPen(element.StrokeIndex));
                switch (element)
                {
                    case PathDrawingElement pathElement:
                        context.DrawPathElement(pathElement, brush, pen);
                        break;
                    case EllipseDrawingElement ellipseElement:
                        context.DrawEllipseElement(ellipseElement, brush, pen);
                        break;
                    case LineDrawingElement lineElement:
                        if (pen is not null) context.DrawLineElement(lineElement, pen);
                        break;
                    case RectDrawingElement rectElement:
                        context.DrawRectElement(rectElement, brush, pen);
                        break;
                }
            }
        }
    }
}
