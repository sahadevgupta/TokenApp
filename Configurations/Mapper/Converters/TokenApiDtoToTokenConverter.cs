using TokenApp.Models;
using TokenApp.Models.Response;

namespace TokenApp.Configurations.Mapper.Converters
{
    /// <summary>Backend TokenApiDto → UI TokenDTO.</summary>
    public class TokenApiDtoToTokenConverter : ConverterBase<TokenApiDto, TokenDTO>
    {
        protected override TokenDTO ConvertImpl(TokenApiDto source)
        {
            var status = TokenApiValueMapper.ToTokenStatus(source.Status);

            return new TokenDTO
            {
                // Id is the local SQLite key and is never taken from the backend.
                RemoteId = source.Id,
                TokenNumber = source.TokenNumber.GetValueOrDefault(),
                Type = TokenApiValueMapper.ToTokenType(source.TokenType),
                DateTime = TokenApiValueMapper.ToDate(source.TokenDate),
                Status = status,
                CreatedAt = TokenApiValueMapper.ToLocalDateTime(source.CreatedAt),
                Applicant = source.User == null
                    ? null
                    : new TokenApplicant
                    {
                        Name = source.User.Name?.Trim() ?? string.Empty,
                        Mobile = source.User.Mobile?.Trim() ?? string.Empty,
                        City = source.User.City?.Trim() ?? string.Empty
                    },
                IsSynced = true,
                AvailableActions = status == TokenStatus.Given
                    ? TokenActions.Print | TokenActions.Reprint | TokenActions.Details
                    : TokenActions.Details
            };
        }
    }
}
