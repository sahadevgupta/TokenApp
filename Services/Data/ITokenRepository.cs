using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TokenApp.Models;

namespace TokenApp.Services.Data
{
    /// <summary>
    /// Repository interface for Token data access
    /// Abstracts the data storage layer from business logic
    /// </summary>
    public interface ITokenRepository
    {
        /// <summary>
        /// Get all tokens from database
        /// </summary>
        Task<List<TokenDTO>> GetAllTokensAsync();

        /// <summary>
        /// Get a token by ID
        /// </summary>
        Task<TokenDTO> GetTokenByIdAsync(int id);

        /// <summary>
        /// Get tokens by type
        /// </summary>
        Task<List<TokenDTO>> GetTokensByTypeAsync(TokenType type);

        /// <summary>
        /// Get tokens by status
        /// </summary>
        Task<List<TokenDTO>> GetTokensByStatusAsync(TokenStatus status);

        /// <summary>
        /// Get tokens by date (exact date match)
        /// </summary>
        Task<List<TokenDTO>> GetTokensByDateAsync(DateTime date);

        /// <summary>
        /// Search tokens by number or location/batch/escort
        /// </summary>
        Task<List<TokenDTO>> SearchTokensAsync(string query);

        /// <summary>
        /// Get tokens with optional filtering and sorting
        /// </summary>
        Task<List<TokenDTO>> GetTokensAsync(
            TokenType? type = null,
            DateTime? date = null,
            string searchQuery = null,
            bool orderByDescending = true);

        /// <summary>
        /// Add a new token
        /// </summary>
        Task<int> AddTokenAsync(TokenDTO token);

        /// <summary>
        /// Update an existing token
        /// </summary>
        Task<bool> UpdateTokenAsync(TokenDTO token);

        /// <summary>
        /// Delete a token by ID
        /// </summary>
        Task<bool> DeleteTokenAsync(int id);

        /// <summary>
        /// Get total token count
        /// </summary>
        Task<int> GetTokenCountAsync();

        /// <summary>
        /// Get token count by type
        /// </summary>
        Task<int> GetTokenCountByTypeAsync(TokenType type);

        /// <summary>
        /// Clear all tokens (for testing/reset)
        /// </summary>
        Task ClearAllAsync();
    }
}
