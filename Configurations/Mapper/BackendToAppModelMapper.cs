using TokenApp.Configurations.Mapper.Converters;
using TokenApp.Models;
using TokenApp.Models.Response;

namespace TokenApp.Configurations.Mapper
{
    /// <summary>
    /// Single entry point for converting backend response models (Models.Response) into app/UI models.
    /// </summary>
    public static class BackendToAppModelMapper
    {
        public static LoginResult GetLoginResult(LoginResponseDto? dto)
        {
            if (dto == null)
            {
                return new LoginResult();
            }
            return new LoginResponseDtoToLoginResultConverter().Convert(dto);
        }

        public static AdminUser GetAdmin(AdminDto? dto)
        {
            if (dto == null)
            {
                return new AdminUser();
            }
            return new AdminDtoToAdminUserConverter().Convert(dto);
        }

        public static IEnumerable<AdminUser> GetAdmins(IEnumerable<AdminDto>? dtos)
        {
            if (dtos == null)
            {
                return new List<AdminUser>();
            }
            var converter = new AdminDtoToAdminUserConverter();
            return dtos.Select(converter.Convert);
        }

        public static TokenDTO GetToken(TokenApiDto? dto)
        {
            if (dto == null)
            {
                return new TokenDTO();
            }
            return new TokenApiDtoToTokenConverter().Convert(dto);
        }

        public static IEnumerable<TokenDTO> GetTokens(IEnumerable<TokenApiDto>? dtos)
        {
            if (dtos == null)
            {
                return new List<TokenDTO>();
            }
            var converter = new TokenApiDtoToTokenConverter();
            return dtos.Select(converter.Convert);
        }

        public static TodayTokenStats GetTodayStats(TodayStatsDto? dto)
        {
            if (dto == null)
            {
                return new TodayTokenStats();
            }
            return new TodayStatsDtoToTodayTokenStatsConverter().Convert(dto);
        }

        public static DashboardSummary GetDashboard(DashboardDataDto? dto)
        {
            if (dto == null)
            {
                return new DashboardSummary();
            }
            return new DashboardDataDtoToDashboardSummaryConverter().Convert(dto);
        }
    }
}
