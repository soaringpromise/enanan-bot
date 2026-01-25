using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Dapper;
using EnananBot.Connector;
using Microsoft.Extensions.Hosting;

namespace EnananBot.Cache;

/// <summary>
/// A hybrid caching service that keeps active server data in memory while persisting changes to SQLite.
/// Implements the "Write-Through" pattern: writes go to DB first, then update Cache.
/// Implements "Lazy Loading": data is fetched from DB only when requested.
/// </summary>
[SuppressMessage("Performance", "CA1822:MarkMembersAsStatic", Justification = "Service is used via Dependency Injection.")]
public sealed class GuildCache : IHostedService
{
    // Thread-safe dictionary to hold currently active/loaded servers
    private static readonly ConcurrentDictionary<ulong, GuildDataCache> Guilds = new();
    
    // Lock objects per server to ensure we don't load the same server from DB twice simultaneously
    private static readonly ConcurrentDictionary<ulong, SemaphoreSlim> GuildLocks = new();

    private sealed class GuildDataCache
    {
        // Stores UserID -> RoleID mappings
        public readonly ConcurrentDictionary<ulong, ulong?> Users = new();
        public ulong? WelcomeChannelId;
        
        // Track when this data was last used for the cache sweeper
        public DateTime LastAccessUtc = DateTime.UtcNow;
    }
    
    // --- IHostedService Implementation ---

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Ensure database tables exist before the bot starts accepting commands
        await InitializeAsync();
        
        // Start the background task to clean up unused memory
        StartCacheSweeper(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Creates the necessary SQLite tables if they do not exist.
    /// </summary>
    private static async Task InitializeAsync()
    {
        await using var conn = new SqLiteConn().GetConnection();
        
        // 'registry': Tracks which users belong to which server and their optional specific role
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS registry (
                guild_id INTEGER NOT NULL,
                user_id  INTEGER NOT NULL,
                role_id  INTEGER,
                PRIMARY KEY (guild_id, user_id)
            );

            CREATE INDEX IF NOT EXISTS idx_registry_user
            ON registry (user_id);
        """);
        
        // 'welcome': Stores configuration for welcome messages per server
        await conn.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS welcome (
                guild_id INTEGER PRIMARY KEY,
                channel_id INTEGER
            );
        """);
    }
    
    // --- Public API ---

    /// <summary>
    /// Retrieves the cached Role ID for a specific user in a server.
    /// </summary>
    public async Task<ulong?> GetRoleAsync(ulong guildId, ulong userId)
    {
        var cache = await GetGuildCacheAsync(guildId);
        return cache.Users.GetValueOrDefault(userId);
    }

    public static async Task<IReadOnlyDictionary<ulong, ulong?>> GetAllRolesAsync(ulong guildId)
    {
        var cache = await GetGuildCacheAsync(guildId);
        return cache.Users;
    }

    /// <summary>
    /// Checks if a server exists in the database without loading the entire cache.
    /// </summary>
    public async Task<bool> IsGuildRegisteredAsync(ulong guildId)
    {
        const string sql = """
            SELECT 1
            FROM registry
            WHERE guild_id = @GuildId
            LIMIT 1;
        """;

        await using var conn = new SqLiteConn().GetConnection();
        // Note: Casting ulong to long because SQLite does not support unsigned 64-bit integers
        var result = await conn.ExecuteScalarAsync<long?>(sql, new
        {
            GuildId = unchecked((long)guildId)
        });

        return result.HasValue;
    }

    /// <summary>
    /// Bulk adds users to the registry. Uses a transaction for performance and data integrity.
    /// </summary>
    public async Task<bool> AddUsersAsync(ulong guildId, IReadOnlyCollection<ulong> userIds)
    {
        const string sql = """
            INSERT OR IGNORE INTO registry (guild_id, user_id)
            VALUES (@GuildId, @UserId);
        """;

        await using var conn = new SqLiteConn().GetConnection();
        await using var tx = conn.BeginTransaction();

        var insertedUsers = new List<ulong>();

        foreach (var userId in userIds)
        {
            var affected = await conn.ExecuteAsync(sql, new
            {
                GuildId = unchecked((long)guildId),
                UserId = unchecked((long)userId)
            }, tx);

            if (affected > 0)
                insertedUsers.Add(userId);
        }

        await tx.CommitAsync();

        // Update memory cache only after a successful DB commit
        var cache = await GetGuildCacheAsync(guildId);
        foreach (var userId in insertedUsers)
            cache.Users.TryAdd(userId, null);

        return true;
    }

    public async Task<bool> AddUserAsync(ulong guildId, ulong userId)
    {
        const string sql = """
            INSERT OR IGNORE INTO registry (guild_id, user_id)
            VALUES (@GuildId, @UserId);
        """;

        await using var conn = new SqLiteConn().GetConnection();
        var affected = await conn.ExecuteAsync(sql, new
        {
            GuildId = unchecked((long)guildId),
            UserId = unchecked((long)userId)
        });

        if (affected <= 0) return false;

        var cache = await GetGuildCacheAsync(guildId);
        cache.Users.TryAdd(userId, null);

        return true;
    }

    public async Task RemoveUserAsync(ulong guildId, ulong userId)
    {
        const string sql = """
            DELETE FROM registry
            WHERE guild_id = @GuildId AND user_id = @UserId;
        """;

        await using var conn = new SqLiteConn().GetConnection();
        await conn.ExecuteAsync(sql, new
        {
            GuildId = unchecked((long)guildId),
            UserId = unchecked((long)userId)
        });

        // Try removing from cache if it exists in memory
        if (Guilds.TryGetValue(guildId, out var cache))
            cache.Users.TryRemove(userId, out _);
    }

    public async Task<bool> SetRoleAsync(ulong guildId, ulong userId, ulong roleId)
    {
        const string sql = """
            UPDATE registry
            SET role_id = @RoleId
            WHERE guild_id = @GuildId AND user_id = @UserId;
        """;

        await using var conn = new SqLiteConn().GetConnection();
        var affected = await conn.ExecuteAsync(sql, new
        {
            GuildId = unchecked((long)guildId),
            UserId = unchecked((long)userId),
            RoleId = unchecked((long)roleId)
        });

        if (affected <= 0) return false;

        var cache = await GetGuildCacheAsync(guildId);
        cache.Users[userId] = roleId;

        return true;
    }

    public async Task<bool> ClearRoleAsync(ulong guildId, ulong userId)
    {
        const string sql = """
            UPDATE registry
            SET role_id = NULL
            WHERE guild_id = @GuildId AND user_id = @UserId;
        """;

        await using var conn = new SqLiteConn().GetConnection();
        var affected = await conn.ExecuteAsync(sql, new
        {
            GuildId = unchecked((long)guildId),
            UserId = unchecked((long)userId)
        });

        if (affected <= 0) return false;

        var cache = await GetGuildCacheAsync(guildId);
        cache.Users[userId] = null;

        return true;
    }

    // --- Welcome Channel Management ---

    public async Task<ulong?> GetWelcomeChannelAsync(ulong guildId)
    {
        var cache = await GetGuildCacheAsync(guildId);
        return cache.WelcomeChannelId;
    }

    public async Task<bool> SetWelcomeChannelAsync(ulong guildId, ulong channelId)
    {
        const string sql = """
            INSERT INTO welcome (guild_id, channel_id)
            VALUES (@GuildId, @ChannelId)
            ON CONFLICT(guild_id) DO UPDATE SET channel_id = excluded.channel_id;
        """;

        await using var conn = new SqLiteConn().GetConnection();
        var affected = await conn.ExecuteAsync(sql, new
        {
            GuildId = unchecked((long)guildId),
            ChannelId = unchecked((long)channelId)
        });

        if (affected <= 0) return false;

        var cache = await GetGuildCacheAsync(guildId);
        cache.WelcomeChannelId = channelId;

        return true;
    }

    public async Task<bool> ClearWelcomeChannelAsync(ulong guildId)
    {
        const string sql = """
            UPDATE welcome
            SET channel_id = NULL
            WHERE guild_id = @GuildId;
        """;

        await using var conn = new SqLiteConn().GetConnection();
        var affected = await conn.ExecuteAsync(sql, new { GuildId = unchecked((long)guildId) });

        if (affected <= 0) return false;

        var cache = await GetGuildCacheAsync(guildId);
        cache.WelcomeChannelId = null;

        return true;
    }

    /// <summary>
    /// Completely removes a server from both DB and Cache.
    /// </summary>
    public async Task RemoveGuildAsync(ulong guildId)
    {
        const string sqlRegistry = "DELETE FROM registry WHERE guild_id = @GuildId;";
        const string sqlWelcome = "DELETE FROM welcome WHERE guild_id = @GuildId;";

        await using var conn = new SqLiteConn().GetConnection();
        await conn.ExecuteAsync(sqlRegistry, new { GuildId = unchecked((long)guildId) });
        await conn.ExecuteAsync(sqlWelcome, new { GuildId = unchecked((long)guildId) });

        Guilds.TryRemove(guildId, out _);
        if (GuildLocks.TryRemove(guildId, out var gate))
            gate.Dispose();
    }

    // --- Internal Caching Logic ---

    /// <summary>
    /// Core method: Returns the cache for a server. If not in memory, loads it from DB.
    /// Uses SemaphoreSlim to ensure thread safety during loading.
    /// </summary>
    private static async Task<GuildDataCache> GetGuildCacheAsync(ulong guildId)
    {
        // 1. Fast Path: Check if already in memory
        if (Guilds.TryGetValue(guildId, out var cache))
        {
            cache.LastAccessUtc = DateTime.UtcNow;
            return cache;
        }

        // 2. Slow Path: Acquire lock to load from DB
        var gate = GuildLocks.GetOrAdd(guildId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync();

        try
        {
            // Double-check: Another thread might have loaded it while we waited for the lock
            if (Guilds.TryGetValue(guildId, out cache))
            {
                cache.LastAccessUtc = DateTime.UtcNow;
                return cache;
            }

            // Load from DB and store in memory
            var loaded = await LoadGuildFromDbAsync(guildId);
            Guilds[guildId] = loaded;
            return loaded;
        }
        finally
        {
            gate.Release();
        }
    }

    private static async Task<GuildDataCache> LoadGuildFromDbAsync(ulong guildId)
    {
        var cache = new GuildDataCache();
        
        const string sqlUsers = """
            SELECT user_id, role_id
            FROM registry
            WHERE guild_id = @GuildId;
        """;

        await using var conn = new SqLiteConn().GetConnection();
        var rows = await conn.QueryAsync<(long UserId, long? RoleId)>(sqlUsers, new
        {
            GuildId = unchecked((long)guildId)
        });

        // Convert DB longs back to ulongs
        foreach (var row in rows)
            cache.Users[unchecked((ulong)row.UserId)] = row.RoleId.HasValue ? unchecked((ulong)row.RoleId.Value) : null;
        
        const string sqlWelcome = "SELECT channel_id FROM welcome WHERE guild_id = @GuildId LIMIT 1;";
        var welcomeId = await conn.ExecuteScalarAsync<long?>(sqlWelcome, new { GuildId = unchecked((long)guildId) });
        cache.WelcomeChannelId = welcomeId.HasValue ? unchecked((ulong)welcomeId.Value) : null;

        return cache;
    }

    // --- Background Maintenance ---

    /// <summary>
    /// Periodically cleans up servers from memory that haven't been accessed recently.
    /// This prevents memory bloat for servers that are inactive.
    /// </summary>
    private void StartCacheSweeper(CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(10), ct);
                SweepInactiveGuilds();
            }
        }, ct);
    }

    private static void SweepInactiveGuilds()
    {
        // Unload servers not accessed in the last 30 minutes
        var cutoff = DateTime.UtcNow - TimeSpan.FromMinutes(30);

        foreach (var kv in Guilds)
        {
            if (kv.Value.LastAccessUtc < cutoff)
                Guilds.TryRemove(kv.Key, out _);
        }
    }
}