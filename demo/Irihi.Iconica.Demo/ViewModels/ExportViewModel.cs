using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Irihi.Iconica.Demo.Models;

namespace Irihi.Iconica.Demo.ViewModels;

public partial class ExportViewModel : ObservableObject
{
    private readonly IExportSettings _settings;
    private readonly IconInfo? _iconInfo;

    [ObservableProperty] public partial string? GlobalStyle { get; set; }
    [ObservableProperty] public partial string? GlobalResource { get; set; }
    [ObservableProperty] public partial string? Icon { get; set; }
    [ObservableProperty] public partial bool UseLocalColor { get; set; }
    [ObservableProperty] public partial bool HasIconInfo { get; set; }

    public string XmlNamespace => _settings.XmlNamespace;

    public ExportViewModel(IExportSettings settings, IconInfo? iconType)
    {
        _settings = settings;
        _iconInfo = iconType;
        HasIconInfo = iconType is not null;
        GenerateGlobalStyle();
        GenerateGlobalResource();
        GenerateIcon();
    }

    private void GenerateGlobalStyle()
    {
        var setters = string.Join("\n",
            _settings.ExportColors.Select(c =>
                $$"""
                      <Setter Property="{{c.Property}}" Value="{DynamicResource Icon{{c.Property}}Brush}" />
                  """));

        GlobalStyle =
            $"""
             <Style Selector=":is({_settings.StyleSelector})">
             {setters}
             </Style>
             """;
    }

    private void GenerateGlobalResource()
    {
        var light = string.Join("\n",
            _settings.ExportColors.Select(c =>
                $"""
                             <SolidColorBrush x:Key="Icon{c.Property}Brush" Color="{c.LightColor}" />
                 """));
        var dark = string.Join("\n",
            _settings.ExportColors.Select(c =>
                $"""
                             <SolidColorBrush x:Key="Icon{c.Property}Brush" Color="{c.DarkColor}" />
                 """));

        GlobalResource =
            $"""
             <ResourceDictionary>
                 <ResourceDictionary.ThemeDictionaries>
                     <ResourceDictionary x:Key="Light">
             {light}
                     </ResourceDictionary>
                     <ResourceDictionary x:Key="Dark">
             {dark}
                     </ResourceDictionary>
                 </ResourceDictionary.ThemeDictionaries>
             </ResourceDictionary>
             """;
    }

    private void GenerateIcon()
    {
        if (_iconInfo is null) return;
        var element =
            $"""
             <{_settings.ElementPrefix}:{_iconInfo.ClassName}
                 Width="{_settings.Size}"
                 Height="{_settings.Size}"
             """;
        if (UseLocalColor)
        {
            var colorProps = string.Join("\n",
                _settings.ExportColors.Select(c =>
                    $$"""
                          {{c.Property}}="{DynamicResource Icon{{c.Property}}Brush}"
                      """));
            element =
                $"""
                 {element}
                 {colorProps}
                 """;
        }

        // Only emit attributes the icon type actually has (e.g. Semi lab icons have no
        // stroke properties, and sets without color modes produce no Mode attribute).
        var hasStroke = _iconInfo.IconType?.GetProperty("StrokeWidth") is not null;
        if (hasStroke)
        {
            element =
                $"""
                 {element}
                     StrokeWidth="{_settings.StrokeWidth}"
                     LineCap="Round"
                     LineJoin="Round"
                 """;
        }

        if (!string.IsNullOrEmpty(_settings.Mode))
        {
            element =
                $"""
                 {element}
                     Mode="{_settings.Mode}"
                 """;
        }

        Icon = $"{element} />";
    }

    partial void OnUseLocalColorChanged(bool value)
    {
        GenerateIcon();
    }
}