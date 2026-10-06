using System;
using TokenApp.Models;
using TokenApp.Models.Database;

namespace TokenApp.Services.Data
{
    /// <summary>
    /// Interface for mapping between database and UI models
    /// </summary>
    public interface ITokenMapper
    {
        TokenDTO MapToDTO(DBToken dbToken);
        DBToken MapToDBToken(TokenDTO token);
    }

    /// <summary>
    /// Implementation of token mapper
    /// Converts between DBToken (database model) and TokenDTO (UI model)
    /// </summary>
    public class TokenMapper : ITokenMapper
    {
        public TokenDTO MapToDTO(DBToken dbToken)
        {
            if (dbToken == null)
                return null;

            return new TokenDTO
            {
                Id = dbToken.Id,
                TokenNumber = dbToken.TokenNumber,
                Type = dbToken.TypeEnum,
                DateTime = new DateTime(dbToken.DateTime, DateTimeKind.Utc),
                Status = dbToken.StatusEnum,
                Location = dbToken.Location,
                BatchNumber = dbToken.BatchNumber,
                Escort = dbToken.Escort,
                AvailableActions = dbToken.ActionsEnum
            };
        }

        public DBToken MapToDBToken(TokenDTO token)
        {
            if (token == null)
                return null;

            var now = DateTime.UtcNow;
            return new DBToken
            {
                Id = token.Id,
                TokenNumber = token.TokenNumber,
                TypeEnum = token.Type,
                DateTime = token.DateTime.Ticks,
                StatusEnum = token.Status,
                Location = token.Location,
                BatchNumber = token.BatchNumber,
                Escort = token.Escort,
                ActionsEnum = token.AvailableActions,
                UpdatedAtDateTime = now
            };
        }
    }
}
