using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Irihi.Iconica.Demo.Views;
using Ursa.Controls;

namespace Irihi.Iconica.Demo.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        IconParkPage = new IconParkPageViewModel();
        TDesignPage = new TDesignPageViewModel();
        SemiPage = new SemiPageViewModel();
    }

    public IconParkPageViewModel IconParkPage { get; }
    public TDesignPageViewModel TDesignPage { get; }
    public SemiPageViewModel SemiPage { get; }

    public string[] PageNames { get; } = ["IconPark", "TDesign", "Semi"];

    [ObservableProperty] public partial int SelectedTabIndex { get; set; }
    [ObservableProperty] public partial bool IconParkPageVisible { get; set; } = true;
    [ObservableProperty] public partial bool TDesignPageVisible { get; set; }
    [ObservableProperty] public partial bool SemiPageVisible { get; set; }

    partial void OnSelectedTabIndexChanged(int value)
    {
        IconParkPageVisible = value == 0;
        TDesignPageVisible = value == 1;
        SemiPageVisible = value == 2;
    }

    [RelayCommand]
    private async Task OnSaveDialogAsync()
    {
        var settings = SelectedTabIndex switch
        {
            0 => IconParkPage.SettingPanel,
            1 => TDesignPage.SettingPanel,
            _ => SemiPage.SettingPanel,
        };
        var iconInfo = SelectedTabIndex switch
        {
            0 => IconParkPage.Detail.IconInfo,
            1 => TDesignPage.Detail.IconInfo,
            _ => SemiPage.Detail.IconInfo,
        };
        var vm = new ExportViewModel(settings, iconInfo);
        await OverlayDialog.ShowStandardAsync<ExportView, ExportViewModel>(vm,
            options: new OverlayDialogOptions
            {
                Title = "获取样式",
                Buttons = DialogButton.None
            });
    }
}