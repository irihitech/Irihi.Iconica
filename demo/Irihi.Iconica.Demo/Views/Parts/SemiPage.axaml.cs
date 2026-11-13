using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.Messaging;
using Irihi.Iconica.Demo.Messages;
using Irihi.Iconica.Demo.Models;
using Irihi.Iconica.Demo.ViewModels;

namespace Irihi.Iconica.Demo.Views;

public partial class SemiPage : UserControl
{
    public SemiPage()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<SemiPage, ColorResourceChangeMessage, string>(this,
            MessengerChannels.Semi, OnColorResourceChanged);
        WeakReferenceMessenger.Default.Register<SemiPage, SizeResourceChangeMessage, string>(this,
            MessengerChannels.Semi, OnSizeResourceChanged);
        WeakReferenceMessenger.Default.Register<SemiPage, SemiIconGroupResourceChangeMessage, string>(this,
            MessengerChannels.Semi, OnGroupChanged);
    }

    private void OnGroupChanged(SemiPage recipient, SemiIconGroupResourceChangeMessage message)
    {
        if (DataContext is SemiPageViewModel vm)
        {
            vm.Viewer.Filter = message.Value switch
            {
                SemiIconGroup.Fill => IconInfo.IsSemiFill,
                SemiIconGroup.Stroked => IconInfo.IsSemiStroked,
                SemiIconGroup.AI => IconInfo.IsSemiAi,
                SemiIconGroup.Lab => IconInfo.IsSemiLab,
                _ => null,
            };
        }
    }

    private void OnSizeResourceChanged(SemiPage recipient, SizeResourceChangeMessage message)
    {
        // Only the detail preview consumes IconSize; the grid stays at the 24px default.
        Resources["IconSize"] = message.Value;
    }

    private void OnColorResourceChanged(SemiPage recipient, ColorResourceChangeMessage message)
    {
        // 仅在页面处于可视树中时 ActualThemeVariant 才是准确的；
        // 非激活页面写入的旧主题条目会在页面重新加载（OnLoaded）后由主题消息校正。
        if (message.Value is not null &&
            Resources.ThemeDictionaries[ActualThemeVariant] is ResourceDictionary rd)
        {
            rd[message.ResourceKey] = new ImmutableSolidColorBrush(message.Value.Value);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // 触发设置面板按当前主题重发颜色，校正本页资源。
        WeakReferenceMessenger.Default.Send(ActualThemeVariant);
    }
}
