using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using Irihi.Iconica.Demo.Models;

namespace Irihi.Iconica.Demo.Views;

public partial class SemiIconBriefView : UserControl
{
    public SemiIconBriefView()
    {
        InitializeComponent();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is IconInfo info)
        {
            WeakReferenceMessenger.Default.Send(info);
        }
    }
}
