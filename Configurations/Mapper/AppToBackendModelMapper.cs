using TokenApp.Models;
using TokenApp.Models.Request;

namespace TokenApp.Configurations.Mapper
{
    /// <summary>
    /// Converts app/UI models into backend request models (Models.Request).
    /// </summary>
    public static class AppToBackendModelMapper
    {
        /// <summary>Returns null when the token type cannot be generated through the documented API.</summary>
        public static GenerateTokenRequestDto? GetGenerateTokenRequest(TokenDTO token)
        {
            var apiType = TokenApiValueMapper.ToApiTokenType(token.Type);
            if (apiType == null)
            {
                return null;
            }

            return new GenerateTokenRequestDto
            {
                TokenType = apiType,
                TokenNumber = token.TokenNumber,
                TokenDate = TokenApiValueMapper.ToApiDate(token.DateTime)
            };
        }

        public static ApplyTokenRequestDto? GetApplyTokenRequest(TokenType type)
        {
            var apiType = TokenApiValueMapper.ToApiTokenType(type);
            return apiType == null ? null : new ApplyTokenRequestDto { TokenType = apiType };
        }
    }
}
