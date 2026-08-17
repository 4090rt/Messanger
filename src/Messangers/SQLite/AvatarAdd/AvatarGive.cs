using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using System;
using System.Data.SQLite;

namespace Messangers.SQLite.AvatarAdd
{
    public class AvatarGive
    {
        private readonly ILogger<AvatarGive> _logger;
        private readonly PoolSQLite _poolSQLite;
        private readonly IMemoryCache _memoryCache;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly InvalidExcaptionDelegate _invalidExcaptionDelegate;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private bool _Is_chekedindex = false;

        public AvatarGive(ILogger<AvatarGive> logger, PoolSQLite poolSQLite, IMemoryCache memoryCache,
            SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate, InvalidExcaptionDelegate invalidExcaptionDelegate)
        { 
            _logger = logger;
            _poolSQLite = poolSQLite;
            _memoryCache = memoryCache;
            _exceptionDelegate = exceptionDelegate;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _invalidExcaptionDelegate = invalidExcaptionDelegate;

            Task.Run(async () => await Inithiakuzate()).ConfigureAwait(false);
        }

        public async Task Inithiakuzate()
        {
            if (_Is_chekedindex == true) return;

            if (_Is_chekedindex == false)
            {
                await CreateIndex().ConfigureAwait(false);
                bool result = await IndexProverka().ConfigureAwait(false);

                _Is_chekedindex = result;
            }
        }

        public async Task<ReadOnlyMemory<byte>> CachaRequest(string username)
        {
            string cache_key = "Avatar_byte_cache_key";
            string stale_cache_key = "StaleAvatar_byte_cache_key";
            ReadOnlyMemory<byte> oldcache = null;

            if (_memoryCache.TryGetValue(cache_key, out ReadOnlyMemory<byte> cached))
            {
                oldcache = cached;
                return cached;
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_memoryCache.TryGetValue(cache_key, out ReadOnlyMemory<byte> cached2))
                {
                    return cached2;
                }

                var fallback = Policy<ReadOnlyMemory<byte>>
                    .Handle<Exception>()
                    .OrResult(r => r.Length == 0)
                    .FallbackAsync(
                        fallbackAction: async (outcome, context, ctx) =>
                        {
                            var exceptin = outcome.Exception;
                            var isEmpty = outcome.Result.Length == 0;

                            if (exceptin != null)
                            {
                                _logger.LogWarning($"⚠️ Fallback by exception: {exceptin.Message}");
                                return default;
                            }
                            else if (isEmpty)
                            {
                                _logger.LogWarning($"⚠️ Fallback by empty result");
                                return default;
                            }
                            else if (oldcache.Length != 0)
                            {
                                _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                                return oldcache;
                            }
                            else if (_memoryCache.TryGetValue(stale_cache_key, out ReadOnlyMemory<byte> stalecached))
                            {
                                _logger.LogInformation($"✅ Returning stale copy for {stalecached}");
                                return stalecached;
                            }
                            else
                            {
                                _logger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю default");
                                return default;
                            }
                        },
                        onFallbackAsync: async (outcome, ctx) =>
                        {
                            _logger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                            await Task.CompletedTask;
                        });

                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    ReadOnlyMemory<byte> readOnlyMemory = await Rquest(username).ConfigureAwait(false);

                    if (readOnlyMemory.Length == 0)
                        return default;

                    var memoryoptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                    _memoryCache.Set(cache_key, readOnlyMemory, memoryoptions);

                    var stalememoryoptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15));

                    _memoryCache.Set(stale_cache_key, readOnlyMemory, stalememoryoptions);

                    return readOnlyMemory;
                });
                return fallbackresult;

            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new ReadOnlyMemory<byte>();
            }
            finally
            { 
                _semaphore.Release();
            }
        }

        public async Task<ReadOnlyMemory<byte>> Rquest(string username)
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLite.CreateConnection();
                string command = "SELECT Avatar FROM RegisterBase WHERE Login = @L";

                await using (SQLiteCommand commandsql = new SQLiteCommand(command, connection))
                {
                    commandsql.Parameters.AddWithValue("@L", username);

                    object bytes = commandsql.ExecuteScalarAsync().ConfigureAwait(false);

                    if (bytes == null)
                        return new ReadOnlyMemory<byte>();

                    byte[] allbytes = bytes as byte[];
                    ReadOnlyMemory<byte> byteread = allbytes.AsMemory();

                    return byteread;
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
                return new ReadOnlyMemory<byte>();

            }
            catch (InvalidOperationException ex)
            {
                await _invalidExcaptionDelegate.RunDelegate(ex).ConfigureAwait(false);
                return new ReadOnlyMemory<byte>();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex).ConfigureAwait(false);
                return new ReadOnlyMemory<byte>();
            }
            finally
            { 
                if (connection != null)
                {
                    _poolSQLite.CloseConnection(connection);
                }
            }
        }

        public async Task CreateIndex()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLite.CreateConnection();

                string command = "CREATE INDEX IF NOT EXISTS IX_RegisterBase_gIVEaVATAR ON RegisterBase";

                await using (SQLiteCommand sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
                return;

            }
            catch (InvalidOperationException ex)
            {
                await _invalidExcaptionDelegate.RunDelegate(ex).ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex).ConfigureAwait(false);
                return;
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLite.CloseConnection(connection);
                }
            }
        }

        public async Task<bool> IndexProverka()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLite.CreateConnection();

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_RegisterBase_gIVEaVATAR' AND tbl_name = 'RegisterBse'";

                await using (SQLiteCommand sQLcommand = new SQLiteCommand(command, connection))
                { 
                    var result = await sQLcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    bool exec = Convert.ToInt32(result) == 1;
                    if (exec)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
                return false;

            }
            catch (InvalidOperationException ex)
            {
                await _invalidExcaptionDelegate.RunDelegate(ex).ConfigureAwait(false);
                return false;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex).ConfigureAwait(false);
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLite.CloseConnection(connection);
                }
            }
        }
    }
}
