using Irihi.Iconica.Demo.Models;

namespace Irihi.Iconica.Demo.ViewModels;

public class SemiPageViewModel
{
    public IconViewerViewModel Viewer { get; }
    public IconDetailViewModel Detail { get; }
    public SemiSettingPanelViewModel SettingPanel { get; }

    public SemiPageViewModel()
    {
        // Default to the Fill family; the toolbar selector keeps in sync via messages.
        Viewer = new IconViewerViewModel(IconInfo.SemiIconInfos, "IRIHI ICONICA 系列图标 - Semi")
        {
            Filter = IconInfo.IsSemiFill,
        };
        Detail = new IconDetailViewModel(IconInfo.SemiIconInfos);
        SettingPanel = new SemiSettingPanelViewModel();
    }
}
