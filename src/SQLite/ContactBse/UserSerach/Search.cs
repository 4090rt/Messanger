using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using System;
using System.Data.SQLite;

namespace Messangers.SQLite.ContactBse.UserSerach
{
    public class Search
    {
        private readonly ILogger<Search> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly IMemoryCache _memorycache;
        private bool _Is_chekedindex = false;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1);
 
        public Search(ILogger<Search> logger, PoolSQLite poolSQLiteConnection,
            SQLiteExceptionDelegate sQLiteExceptionDelegate, 
            ExceptionDelegate exceptionDelegate, IMemoryCache memoryCache)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
            _memorycache = memoryCache;

            Task.Run(async () => await Inithializate()).ConfigureAwait(false);
        }

        public async Task Inithializate()
        {
            if (_Is_chekedindex) return;

            if (_Is_chekedindex = false)
            {
                await createIndex();
                await indexproverka();
            }
            _Is_chekedindex = true;
        }

        public async Task<bool> RequestCache(string username)
        {
            string cachekey = $"cache_key{username}search";
            string stalecache = $"stale{cachekey}";
            bool oldcache = false;
            if (_memorycache.TryGetValue(cachekey, out bool cached))
            { 
                oldcache = cached;
                return cached;
            }

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (_memorycache.TryGetValue(cachekey, out bool cached2))
                { 
                    return cached2;
                }

                var fallback = Policy<bool>
                    .Handle<Exception>()
                    .OrResult(r => r == null)
                    .FallbackAsync(
                    fallbackAction: async (outcome, context, ctx) =>
                    {
                        var exception = outcome.Exception;
                        var isEmpty = outcome.Result == null;

                        if (exception != null)
                        {
                            _logger.LogWarning($"⚠️ Fallback by exception: {exception.Message}");
                        }
                        if (isEmpty)
                        {
                            _logger.LogWarning($"⚠️ Fallback by empty result");
                        }
                        if (oldcache != null)
                        {
                            _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                            return oldcache;
                        }
                        if (_memorycache.TryGetValue(stalecache, out bool stalecached))
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
                    var result = await Request(username).ConfigureAwait(false);

                    var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                    _memorycache.Set(cachekey, result, options);

                    var staleoptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                    _memorycache.Set(stalecache, result, staleoptions);

                    return result;
                });
                return fallbackresult;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return false;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<bool> Request(string username)
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT COUNT(*) FROM RegisterBase WHERE Login = @Login";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@Login", username);

                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);
                    int count = Convert.ToInt32(result);
                    return count > 0;
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                return false;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }

        public async Task createIndex()
        {
            SQLiteConnection connection = null;

            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_RegisterBase_Search ON RegisterBase(Login)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogWarning("Индекс  IX_RegisterBase_Search создан");
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }

        public async Task<bool> indexproverka()
        {
            SQLiteConnection connection = null;

            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_RegisterBase_Search' AND tbl_name = 'RegisterBase";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);
                    if (result != null)
                    {
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
                    else
                    {
                        return false;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                return false;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }
    }
}
