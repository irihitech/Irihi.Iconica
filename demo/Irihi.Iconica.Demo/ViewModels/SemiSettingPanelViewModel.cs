using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Irihi.Iconica.Demo.Messages;

namespace Irihi.Iconica.Demo.ViewModels;

public partial class SemiSettingPanelViewModel : ObservableObject, IExportSettings
{
    private readonly Color? _lightDefaultPrimaryColor = Colors.Black;
    private readonly Color? _lightDefaultSecondaryColor = Color.Parse("#A647FF");
    private readonly Color? _darkDefaultPrimaryColor = Colors.White;
    private readonly Color? _darkDefaultSecondaryColor = Color.Parse("#A647FF");

    internal Color? _lightPrimaryColor = Colors.Black;
    internal Color? _lightSecondaryColor = Color.Parse("#A647FF");
    internal Color? _darkPrimaryColor = Colors.White;
    internal Color? _darkSecondaryColor = Color.Parse("#A647FF");

    private ThemeVariant? _currentThemeVariant;

    [ObservableProperty] public partial Color? PrimaryColor { get; set; } = Colors.Black;
    [ObservableProperty] public partial Color? SecondaryColor { get; set; } = Color.Parse("#A647FF");
    [ObservableProperty] public partial double Size { get; set; }

    /// <summary>
    /// Not a user setting: Semi stroke widths are fixed values baked into each SVG.
    /// Exposed only so exports carry the SemiIconBase default (2).
    /// </summary>
    double IExportSettings.StrokeWidth => 2;

    public SemiSettingPanelViewModel()
    {
        WeakReferenceMessenger.Default.Register<SemiSettingPanelViewModel, ThemeVariant>(this, OnActualThemeChanged);
        Size = 24;
    }

    partial void OnPrimaryColorChanged(Color? value)
    {
        WeakReferenceMessenger.Default.Send(new ColorResourceChangeMessage(value, nameof(PrimaryColor)),
            MessengerChannels.Semi);
        SaveToThemeCache(value, ref _lightPrimaryColor, ref _darkPrimaryColor);
    }

    partial void OnSecondaryColorChanged(Color? value)
    {
        WeakReferenceMessenger.Default.Send(new ColorResourceChangeMessage(value, nameof(SecondaryColor)),
            MessengerChannels.Semi);
        SaveToThemeCache(value, ref _lightSecondaryColor, ref _darkSecondaryColor);
    }

    #region Groups

    [ObservableProperty] public partial SemiIconGroup SelectedGroup { get; set; } = SemiIconGroup.Fill;

    /// <summary>Only AI icons use the Secondary slot; hide its picker otherwise.</summary>
    [ObservableProperty] public partial bool ShowSecondaryColor { get; private set; }

    public ObservableCollection<SemiIconGroup> Groups { get; set; }
        = [SemiIconGroup.Fill, SemiIconGroup.Stroked, SemiIconGroup.AI, SemiIconGroup.Lab];

    #endregion

    partial void OnSizeChanged(double value)
    {
        WeakReferenceMessenger.Default.Send(new SizeResourceChangeMessage(value), MessengerChannels.Semi);
    }

    partial void OnSelectedGroupChanged(SemiIconGroup value)
    {
        ShowSecondaryColor = value == SemiIconGroup.AI;
        WeakReferenceMessenger.Default.Send(new SemiIconGroupResourceChangeMessage(value), MessengerChannels.Semi);
    }

    private void SaveToThemeCache(Color? value, ref Color? light, ref Color? dark)
    {
        if (_currentThemeVariant == ThemeVariant.Light)
        {
            light = value;
        }
        else if (_currentThemeVariant == ThemeVariant.Dark)
        {
            dark = value;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        if (_currentThemeVariant == ThemeVariant.Light)
        {
            PrimaryColor = _lightDefaultPrimaryColor;
            SecondaryColor = _lightDefaultSecondaryColor;
        }
        else if (_currentThemeVariant == ThemeVariant.Dark)
        {
            PrimaryColor = _darkDefaultPrimaryColor;
            SecondaryColor = _darkDefaultSecondaryColor;
        }
    }

    private void OnActualThemeChanged(SemiSettingPanelViewModel recipient, ThemeVariant message)
    {
        _currentThemeVariant = message;
        if (message == ThemeVariant.Light)
        {
            PrimaryColor = _lightPrimaryColor;
            SecondaryColor = _lightSecondaryColor;
        }
        else if (message == ThemeVariant.Dark)
        {
            PrimaryColor = _darkPrimaryColor;
            SecondaryColor = _darkSecondaryColor;
        }
    }

    #region IExportSettings

    public string XmlNamespace => "https://irihi.tech/iconica/semi";
    public string ElementPrefix => "semiicon";
    public string StyleSelector => "semiicon|SemiIconBase";
    public string Mode => string.Empty;

    public IReadOnlyList<ExportColor> ExportColors =>
    [
        new("Primary", _lightPrimaryColor!.Value.ToString(), _darkPrimaryColor!.Value.ToString()),
        new("Secondary", _lightSecondaryColor!.Value.ToString(), _darkSecondaryColor!.Value.ToString())
    ];

    #endregion
}
