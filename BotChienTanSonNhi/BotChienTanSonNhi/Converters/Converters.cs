using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BotChienTanSonNhi.Converters;

public class EqualityToBrushConverter : IValueConverter
{
    public IBrush ActiveBrush { get; set; } = new SolidColorBrush(Color.Parse("#F59E0B")); // Amber Gold
    public IBrush InactiveBrush { get; set; } = new SolidColorBrush(Color.Parse("#262A34")); // Dark Slate
    public IBrush ActiveTextBrush { get; set; } = new SolidColorBrush(Color.Parse("#111318")); // Dark Text
    public IBrush InactiveTextBrush { get; set; } = new SolidColorBrush(Color.Parse("#D1D5DB")); // Light Text

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null && parameter == null) return ActiveBrush;
        if (value == null || parameter == null) return InactiveBrush;

        bool isEqual = value.ToString() == parameter.ToString();
        return isEqual ? ActiveBrush : InactiveBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EqualityToTextBrushConverter : IValueConverter
{
    public IBrush ActiveTextBrush { get; set; } = new SolidColorBrush(Color.Parse("#000000")); // Black/Dark
    public IBrush InactiveTextBrush { get; set; } = new SolidColorBrush(Color.Parse("#E5E7EB")); // Light Gray

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null && parameter == null) return ActiveTextBrush;
        if (value == null || parameter == null) return InactiveTextBrush;

        bool isEqual = value.ToString() == parameter.ToString();
        return isEqual ? ActiveTextBrush : InactiveTextBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CurrencyFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            return $"{amount:N0} đ";
        }
        if (value is int intAmount)
        {
            return $"{intAmount:N0} đ";
        }
        return "0 đ";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IntToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count && parameter is string targetStr && int.TryParse(targetStr, out int target))
        {
            return count == target;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

