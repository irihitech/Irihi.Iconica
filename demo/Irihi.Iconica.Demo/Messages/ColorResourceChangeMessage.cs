using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Irihi.Iconica.IconPark;
using TDesignIconMode = Irihi.Iconica.TDesign.IconMode;

namespace Irihi.Iconica.Demo.Messages;

public static class MessengerChannels
{
    public const string IconPark = "IconPark";
    public const string TDesign = "TDesign";
    public const string Semi = "Semi";
}

public class ColorResourceChangeMessage(Color? value, string resourceKey) : ValueChangedMessage<Color?>(value)
{
    public string ResourceKey { get; } = resourceKey;
}

public class ModeResourceChangeMessage(IconMode value) : ValueChangedMessage<IconMode>(value)
{
    
}

public class TDesignModeResourceChangeMessage(TDesignIconMode value) : ValueChangedMessage<TDesignIconMode>(value)
{
    
}

public enum IconVariant
{
    Outline,
    Filled
}

public enum SemiIconGroup
{
    Fill,
    Stroked,
    AI,
    Lab,
}

public class SemiIconGroupResourceChangeMessage(SemiIconGroup value) : ValueChangedMessage<SemiIconGroup>(value)
{

}

public class TDesignVariantResourceChangeMessage(IconVariant value) : ValueChangedMessage<IconVariant>(value)
{
    
}

public class SizeResourceChangeMessage(double value) : ValueChangedMessage<double>(value)
{
    
}

public class StrokeWidthResourceChangeMessage(double value) : ValueChangedMessage<double>(value)
{
    
}
