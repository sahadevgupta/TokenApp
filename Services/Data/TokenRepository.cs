using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using TokenApp.Configurations.Mapper;
using TokenApp.Models;
using TokenApp.Models.Database;
using TokenApp.Services.Api;
using TokenApp.Services.Auth;
using TokenApp.Services.Connectivity;
using TokenApp.Services.Database;

namespace TokenApp.Services.Data
{
    /// <summary>
    /// SQLite implementation of ITokenRepository.
    /// SQLite is the source of truth; the backend is reached only through ITokenApiService
    /// and only for syncing queued tokens.
    /// </summary>
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ITokenMapper _mapper;
        private readonly ITokenApiService _api;
        private readonly IConnectivityService _connectivity;
        private readonly ISessionService _session;

        // Serializes local token-number allocation so two rapid generates cannot receive the same number.
        private readonly SemaphoreSlim _generateGate = new(1, 1);
        // Serializes sync runs so the same queued token is never POSTed twice concurrently.
        private readonly SemaphoreSlim _syncGate = new(1, 1);

        public TokenRepository(
            ITokenMapper mapper,
            ITokenApiService api,
            IConnectivityService connectivity,
            ISessionService session)
        {
            _dbContext = AppDbContext.Instance;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        private async Task<SQLite.SQLiteAsyncConnection> GetConnectionAsync()
        {
            await _dbContext.InitializeAsync();
            return _dbContext.GetConnection();
        }

        public async Task<List<TokenDTO>> GetAllTokensAsync()
        {
            try
            {
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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

                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
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
                var connection = await GetConnectionAsync();
                await connection.DeleteAllAsync<DBToken>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ClearAllAsync: {ex.Message}");
            }
        }

        // ---- offline-first generation + sync ---------------------------------------------

        public async Task<ServiceResult<TokenDTO>> GenerateTokenAsync(TokenType type, DateTime? date = null, CancellationToken ct = default)
        {
            if (TokenApiValueMapper.ToApiTokenType(type) == null)
            {
                return ServiceResult<TokenDTO>.Fail(FailureKind.Validation, "Only red and yellow tokens can be generated.");
            }

            int localId;
            await _generateGate.WaitAsync(ct);
            try
            {
                var connection = await GetConnectionAsync();
                var day = (date ?? DateTime.Now).Date;
                var startTicks = day.Ticks;
                var endTicks = day.AddDays(1).Ticks;
                var typeInt = (int)type;

                var last = await connection.Table<DBToken>()
                    .Where(t => t.Type == typeInt && t.DateTime >= startTicks && t.DateTime < endTicks)
                    .OrderByDescending(t => t.TokenNumber)
                    .FirstOrDefaultAsync();

                var now = DateTime.UtcNow;
                var stamp = date.HasValue ? date.Value : DateTime.Now;
                var dbToken = new DBToken
                {
                    TokenNumber = (last?.TokenNumber ?? 0) + 1,
                    TypeEnum = type,
                    DateTime = stamp.Ticks,
                    StatusEnum = TokenStatus.Pending,
                    ActionsEnum = TokenActions.Print | TokenActions.Reprint | TokenActions.Details,
                    IsSynced = false,
                    CreatedAtDateTime = now,
                    UpdatedAtDateTime = now
                };

                await connection.InsertAsync(dbToken);
                localId = dbToken.Id;
            }
            finally
            {
                _generateGate.Release();
            }

            // The token is durable at this point. A sync failure must not turn into a generate failure.
            try
            {
                await SyncPendingAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Caller gave up waiting; the token stays queued and will sync later.
            }

            var saved = await GetTokenByIdAsync(localId);
            return saved == null
                ? ServiceResult<TokenDTO>.Fail(FailureKind.Unknown, "Token could not be saved.")
                : ServiceResult<TokenDTO>.Ok(saved);
        }

        public async Task<int> SyncPendingAsync(CancellationToken ct = default)
        {
            await _syncGate.WaitAsync(ct);
            try
            {
                if (!_connectivity.IsOnline || !await _session.HasSessionAsync())
                {
                    return 0;
                }

                var connection = await GetConnectionAsync();
                var pending = await connection.Table<DBToken>()
                    .Where(t => t.IsSynced == false && t.SyncError == null)
                    .OrderBy(t => t.Id)
                    .ToListAsync();

                var synced = 0;
                foreach (var dbToken in pending)
                {
                    ct.ThrowIfCancellationRequested();

                    var result = await _api.GenerateAndPrintTokenAsync(_mapper.MapToDTO(dbToken), ct);

                    if (result.IsSuccess && result.Data != null)
                    {
                        var now = DateTime.UtcNow.Ticks;
                        await connection.ExecuteAsync(
                            "UPDATE tokens SET is_synced = 1, remote_id = ?, status = ?, sync_timestamp = ?, updated_at = ?, sync_error = NULL WHERE id = ?",
                            result.Data.RemoteId, (int)result.Data.Status, now, now, dbToken.Id);
                        synced++;
                        continue;
                    }

                    if (result.Failure is FailureKind.Validation or FailureKind.Forbidden)
                    {
                        // The backend understood and refused this token; retrying cannot help and
                        // must not block the tokens queued behind it.
                        await connection.ExecuteAsync(
                            "UPDATE tokens SET sync_error = ?, updated_at = ? WHERE id = ?",
                            result.Message ?? "Rejected by server", DateTime.UtcNow.Ticks, dbToken.Id);
                        continue;
                    }

                    // Offline / network / 5xx / 401 / unknown: leave the rest queued and try again on the
                    // next trigger (connectivity restored, app resumed, sign-in, next generate).
                    break;
                }

                return synced;
            }
            finally
            {
                _syncGate.Release();
            }
        }

        public async Task<int> GetPendingSyncCountAsync()
        {
            var connection = await GetConnectionAsync();
            return await connection.Table<DBToken>()
                .Where(t => t.IsSynced == false && t.SyncError == null)
                .CountAsync();
        }
    }
}
