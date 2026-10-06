using System;
using System.Globalization;
using TokenApp.Models;

namespace TokenApp.Converters
{
    /// <summary>
    /// Converts TokenType enum to user-friendly display name
    /// </summary>
    public class TokenTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TokenType tokenType)
            {
                return tokenType switch
                {
                    TokenType.Yellow => "Yellow Token",
                    TokenType.Red => "Red Token",
                    TokenType.Emergency => "Emergency Token",
                    TokenType.Other => "Other Token",
                    _ => "Unknown Token"
                };
            }
            return "Unknown Token";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts TokenType enum to emoji indicator
    /// </summary>
    public class TokenTypeEmojiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TokenType tokenType)
            {
                return tokenType switch
                {
                    TokenType.Yellow => "🟡",
                    TokenType.Red => "🔴",
                    TokenType.Emergency => "⚡",
                    TokenType.Other => "◻️",
                    _ => "?"
                };
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts TokenStatus enum to user-friendly display name
    /// </summary>
    public class TokenStatusNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TokenStatus status)
            {
                return status switch
                {
                    TokenStatus.Given => "Token Given ✓",
                    TokenStatus.SanctumEntry => "Sanctum Entry ✓",
                    TokenStatus.VipEscort => "VIP / Escort ✓",
                    TokenStatus.Pending => "Pending",
                    _ => "Unknown Status"
                };
            }
            return "Unknown Status";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts TokenStatus enum to background color for status badge
    /// </summary>
    public class TokenStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TokenStatus status)
            {
                return status switch
                {
                    TokenStatus.Given => Color.FromArgb("#00C853"), // Green
                    TokenStatus.SanctumEntry => Color.FromArgb("#CC5500"), // Orange
                    TokenStatus.VipEscort => Color.FromArgb("#6A1B9A"), // Purple
                    TokenStatus.Pending => Color.FromArgb("#FFB300"), // Amber
                    _ => Color.FromArgb("#666666") // Gray
                };
            }
            return Color.FromArgb("#666666");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts TokenActions flags to boolean indicating if action is available
    /// </summary>
    public class TokenActionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TokenActions actions && parameter is string actionName)
            {
                return actionName switch
                {
                    "Reprint" => (actions & TokenActions.Reprint) == TokenActions.Reprint,
                    "Details" => (actions & TokenActions.Details) == TokenActions.Details,
                    "Escort" => (actions & TokenActions.Escort) == TokenActions.Escort,
                    "Export" => (actions & TokenActions.Export) == TokenActions.Export,
                    "Print" => (actions & TokenActions.Print) == TokenActions.Print,
                    _ => false
                };
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Checks if a value is not null or empty
    /// </summary>
    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
