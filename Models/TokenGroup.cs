using System.Globalization;

namespace TokenApp.Models
{
    /// <summary>Tokens issued for one token_date; the unit the history list is grouped by.</summary>
    public class TokenGroup : List<TokenDTO>
    {
        public TokenGroup(DateTime date, IEnumerable<TokenDTO> tokens) : base(tokens)
        {
            Title = date == default ? "Unknown date" : date.ToString("ddd, dd MMM yyyy", CultureInfo.CurrentCulture);
            CountText = Count == 1 ? "1 token" : $"{Count} tokens";
        }

        public string Title { get; }
        public string CountText { get; }
    }
}
