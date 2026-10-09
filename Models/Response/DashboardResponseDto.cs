using System.Text.Json.Serialization;

namespace TokenApp.Models.Response
{
    public class DashboardResponseDto : ApiEnvelopeDto
    {
        [JsonPropertyName("data")]
        public DashboardDataDto? Data { get; set; }
    }

    public class DashboardDataDto
    {
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("pending_tokens")]
        public int PendingTokens { get; set; }

        [JsonPropertyName("approved_tokens")]
        public int ApprovedTokens { get; set; }

        [JsonPropertyName("today_stats")]
        public TodayStatsDto? TodayStats { get; set; }

        [JsonPropertyName("tokens")]
        public List<TokenApiDto>? Tokens { get; set; }

        [JsonPropertyName("admins")]
        public List<AdminDto>? Admins { get; set; }
    }

    public class TodayStatsDto
    {
        [JsonPropertyName("last_red")]
        public int LastRed { get; set; }

        [JsonPropertyName("next_red")]
        public int NextRed { get; set; }

        [JsonPropertyName("last_yellow")]
        public int LastYellow { get; set; }

        [JsonPropertyName("next_yellow")]
        public int NextYellow { get; set; }
    }
}
