using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TokenApp.Models;
using TokenApp.Models.Database;
using TokenApp.Services.Database;

namespace TokenApp.Services.Data
{
    /// <summary>
    /// SQLite implementation of ITokenRepository
    /// Handles all token data access from the local database
    /// </summary>
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ITokenMapper _mapper;

        public TokenRepository(ITokenMapper mapper)
        {
            _dbContext = AppDbContext.Instance;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<TokenDTO>> GetAllTokensAsync()
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var dbTokens = await connection.Table<DBToken>().ToListAsync();
                return dbTokens.Select(t => _mapper.MapToDTO(t)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllTokensAsync: {ex.Message}");
                return new List<TokenDTO>();
            }
        }

        public async Task<TokenDTO> GetTokenByIdAsync(int id)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var dbToken = await connection.FindAsync<DBToken>(id);
                return dbToken != null ? _mapper.MapToDTO(dbToken) : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTokenByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TokenDTO>> GetTokensByTypeAsync(TokenType type)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var typeInt = (int)type;
                var dbTokens = await connection.Table<DBToken>()
                    .Where(t => t.Type == typeInt)
                    .ToListAsync();
                return dbTokens.Select(t => _mapper.MapToDTO(t)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTokensByTypeAsync: {ex.Message}");
                return new List<TokenDTO>();
            }
        }

        public async Task<List<TokenDTO>> GetTokensByStatusAsync(TokenStatus status)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var statusInt = (int)status;
                var dbTokens = await connection.Table<DBToken>()
                    .Where(t => t.Status == statusInt)
                    .ToListAsync();
                return dbTokens.Select(t => _mapper.MapToDTO(t)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTokensByStatusAsync: {ex.Message}");
                return new List<TokenDTO>();
            }
        }

        public async Task<List<TokenDTO>> GetTokensByDateAsync(DateTime date)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var startTicks = date.Date.Ticks;
                var endTicks = date.Date.AddDays(1).Ticks;

                var dbTokens = await connection.Table<DBToken>()
                    .Where(t => t.DateTime >= startTicks && t.DateTime < endTicks)
                    .ToListAsync();
                return dbTokens.Select(t => _mapper.MapToDTO(t)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTokensByDateAsync: {ex.Message}");
                return new List<TokenDTO>();
            }
        }

        public async Task<List<TokenDTO>> SearchTokensAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return await GetAllTokensAsync();

                var connection = _dbContext.GetConnection();
                var lowerQuery = query.ToLower();

                // SQLite LIKE is case-insensitive by default
                var dbTokens = await connection.Table<DBToken>()
                    .Where(t => 
                        t.TokenNumber.ToString().Contains(query) ||
                        (t.Location != null && t.Location.ToLower().Contains(lowerQuery)) ||
                        (t.BatchNumber != null && t.BatchNumber.ToLower().Contains(lowerQuery)) ||
                        (t.Escort != null && t.Escort.ToLower().Contains(lowerQuery)))
                    .ToListAsync();

                return dbTokens.Select(t => _mapper.MapToDTO(t)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SearchTokensAsync: {ex.Message}");
                return new List<TokenDTO>();
            }
        }

        public async Task<List<TokenDTO>> GetTokensAsync(
            TokenType? type = null,
            DateTime? date = null,
            string searchQuery = null,
            bool orderByDescending = true)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var query = connection.Table<DBToken>();

                // Apply filters
                if (type.HasValue)
                {
                    var typeInt = (int)type.Value;
                    query = query.Where(t => t.Type == typeInt);
                }

                if (date.HasValue)
                {
                    var startTicks = date.Value.Date.Ticks;
                    var endTicks = date.Value.Date.AddDays(1).Ticks;
                    query = query.Where(t => t.DateTime >= startTicks && t.DateTime < endTicks);
                }

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var lowerQuery = searchQuery.ToLower();
                    query = query.Where(t =>
                        t.TokenNumber.ToString().Contains(searchQuery) ||
                        (t.Location != null && t.Location.ToLower().Contains(lowerQuery)) ||
                        (t.BatchNumber != null && t.BatchNumber.ToLower().Contains(lowerQuery)) ||
                        (t.Escort != null && t.Escort.ToLower().Contains(lowerQuery)));
                }

                // Apply sorting
                var sortedQuery = orderByDescending
                    ? query.OrderByDescending(t => t.DateTime)
                    : query.OrderBy(t => t.DateTime);

                var dbTokens = await sortedQuery.ToListAsync();
                return dbTokens.Select(t => _mapper.MapToDTO(t)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTokensAsync: {ex.Message}");
                return new List<TokenDTO>();
            }
        }

        public async Task<int> AddTokenAsync(TokenDTO token)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var dbToken = _mapper.MapToDBToken(token);
                var now = DateTime.UtcNow;
                dbToken.CreatedAtDateTime = now;
                dbToken.UpdatedAtDateTime = now;

                await connection.InsertAsync(dbToken);
                return dbToken.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddTokenAsync: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateTokenAsync(TokenDTO token)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var dbToken = _mapper.MapToDBToken(token);
                dbToken.UpdatedAtDateTime = DateTime.UtcNow;

                var result = await connection.UpdateAsync(dbToken);
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateTokenAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTokenAsync(int id)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var result = await connection.DeleteAsync<DBToken>(id);
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DeleteTokenAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetTokenCountAsync()
        {
            try
            {
                var connection = _dbContext.GetConnection();
                return await connection.Table<DBToken>().CountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTokenCountAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> GetTokenCountByTypeAsync(TokenType type)
        {
            try
            {
                var connection = _dbContext.GetConnection();
                var typeInt = (int)type;
                return await connection.Table<DBToken>()
                    .Where(t => t.Type == typeInt)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTokenCountByTypeAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task ClearAllAsync()
        {
            try
            {
                var connection = _dbContext.GetConnection();
                await connection.DeleteAllAsync<DBToken>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ClearAllAsync: {ex.Message}");
            }
        }
    }
}
