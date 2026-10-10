using TokenApp.Constants;
using TokenApp.Models;

namespace TokenApp.Configurations.Mapper
{
    /// <summary>
    /// Translates the backend's string vocabulary (token_type / status / token_date) to and from app enums.
    /// </summary>
    public static class TokenApiValueMapper
    {
        public static TokenType ToTokenType(string? apiValue) => apiValue?.Trim().ToLowerInvariant() switch
        {
            ApiConstants.TokenTypeRed => TokenType.Red,
            ApiConstants.TokenTypeYellow => TokenType.Yellow,
            ApiConstants.TokenTypeEmergency => TokenType.Emergency,
            _ => TokenType.Other
        };

        /// <summary>Returns null when the app type has no documented backend value.</summary>
        public static string? ToApiTokenType(TokenType type) => type switch
        {
            TokenType.Red => ApiConstants.TokenTypeRed,
            TokenType.Yellow => ApiConstants.TokenTypeYellow,
            _ => null
        };

        // TokenStatus has no "rejected" value; anything that is not "approved" is shown as Pending.
        public static TokenStatus ToTokenStatus(string? apiValue) =>
            string.Equals(apiValue, ApiConstants.StatusApproved, StringComparison.OrdinalIgnoreCase)
                ? TokenStatus.Given
                : TokenStatus.Pending;

        public static DateTime ToDate(string? apiValue) =>
            DateTime.TryParseExact(apiValue, ApiConstants.TokenDateFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var date)
                ? date
                : default;

        /// <summary>Parses an ISO-8601 UTC timestamp (e.g. created_at) into device local time.</summary>
        public static DateTime? ToLocalDateTime(string? apiValue) =>
            DateTimeOffset.TryParse(apiValue, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal, out var value)
                ? value.LocalDateTime
                : null;

        public static string ToApiDate(DateTime date) =>
            date.ToString(ApiConstants.TokenDateFormat, System.Globalization.CultureInfo.InvariantCulture);
    }
}
