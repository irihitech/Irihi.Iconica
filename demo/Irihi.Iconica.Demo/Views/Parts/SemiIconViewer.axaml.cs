using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Irihi.Iconica.Demo.ViewModels;

namespace Irihi.Iconica.Demo.Views;

public partial class SemiIconViewer : UserControl
{
    private readonly DispatcherTimer _resizeDebounceTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(50),
    };

    public SemiIconViewer()
    {
        InitializeComponent();
        IconsContainer.SizeChanged += OnContainerSizeChanged;
        _resizeDebounceTimer.Tick += (_, _) =>
        {
            _resizeDebounceTimer.Stop();
            // Divide by the cell height (ItemHeight=120 in axaml) so cells stay
            // close to square and each row fits as many briefs as possible.
            ColumnCount = Math.Max(1, (int)(IconsContainer.Bounds.Width / 120));
            Dispatcher.UIThread.Post(() => NavAnchor?.InvalidatePositions());
        };
    }

    private void OnContainerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        _resizeDebounceTimer.Stop();
        _resizeDebounceTimer.Start();
    }

    public static readonly StyledProperty<int> ColumnCountProperty = AvaloniaProperty.Register<SemiIconViewer, int>(
        nameof(ColumnCount));

    public int ColumnCount
    {
        get => GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is IconViewerViewModel vm)
        {
            vm.OnLoad();
        }
    }
}
