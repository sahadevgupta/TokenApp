using System.Collections.Generic;

namespace TokenApp.Models
{
    public class DashboardSummary
    {
        public int TotalTokens { get; set; }
        public int PendingTokens { get; set; }
        public int ApprovedTokens { get; set; }
        public int RedTokens { get; set; }
        public int YellowTokens { get; set; }
        public int EmergencyTokens { get; set; }
        public TodayTokenStats TodayStats { get; set; } = new();
        public List<TokenDTO> Tokens { get; set; } = new();
        public List<AdminUser> Admins { get; set; } = new();
    }

    public class TodayTokenStats
    {
        public int LastRed { get; set; }
        public int NextRed { get; set; }
        public int LastYellow { get; set; }
        public int NextYellow { get; set; }
        public int LastEmergency { get; set; }
        public int NextEmergency { get; set; }
    }
}
