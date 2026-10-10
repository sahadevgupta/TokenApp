using TokenApp.Models;
using TokenApp.Models.Response;

namespace TokenApp.Configurations.Mapper.Converters
{
    public class TodayStatsDtoToTodayTokenStatsConverter : ConverterBase<TodayStatsDto, TodayTokenStats>
    {
        protected override TodayTokenStats ConvertImpl(TodayStatsDto source)
        {
            return new TodayTokenStats
            {
                LastRed = source.LastRed,
                NextRed = source.NextRed,
                LastYellow = source.LastYellow,
                NextYellow = source.NextYellow,
                LastEmergency = source.LastEmergency,
                NextEmergency = source.NextEmergency
            };
        }
    }

    public class DashboardDataDtoToDashboardSummaryConverter : ConverterBase<DashboardDataDto, DashboardSummary>
    {
        protected override DashboardSummary ConvertImpl(DashboardDataDto source)
        {
            return new DashboardSummary
            {
                TotalTokens = source.TotalTokens,
                PendingTokens = source.PendingTokens,
                ApprovedTokens = source.ApprovedTokens,
                RedTokens = source.RedTokens,
                YellowTokens = source.YellowTokens,
                EmergencyTokens = source.EmergencyTokens,
                TodayStats = BackendToAppModelMapper.GetTodayStats(source.TodayStats),
                Tokens = BackendToAppModelMapper.GetTokens(source.Tokens).ToList(),
                Admins = BackendToAppModelMapper.GetAdmins(source.Admins).ToList()
            };
        }
    }
}
