using Messangers.Delegate;
using Messangers.SQLite.ContactBse.UserSave;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using System;
using System.Data.SQLite;

namespace Messangers.SQLite.ContactBse
{
    public class ValidateContact
    {
        private readonly ILogger<ValidateContact> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private bool _Is_chekedindex = false;
        private IMemoryCache _memorycache;
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);


        public ValidateContact(ILogger<ValidateContact> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate,
            ExceptionDelegate exceptionDelegate, IMemoryCache memorycache)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
            _memorycache = memorycache;

            Task.Run(async () => await Inithializate());
        }

        public async Task Inithializate()
        {
            if (_Is_chekedindex) return;

            if (_Is_chekedindex == false)
            {
                await CreateIndex();
                await IdexProverka();
            }

            _Is_chekedindex = true;
        }

        public async Task<string> CacheSearchMethod(string user, string contactname)
        {
            string cache_key = $"cachekey_{user}_{contactname}";
            string stalekey = $"stale{cache_key}";
            string oldcache = "";

            if (_memorycache.TryGetValue(cache_key, out string cached))
            { 
                oldcache = cached;
                return cached;
            }

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (_memorycache.TryGetValue(cache_key, out string cached2))
                {
                    return cached2;
                }

                var fallback = Policy<string>
                    .Handle<Exception>()
                    .OrResult(r => r == null)
                    .FallbackAsync(
                    fallbackAction: async (outcome, context, ctx) =>
                    {
                        var exception = outcome.Exception;
                        var isEty = outcome.Result == null;

                        if (exception != null)
                        {
                            _logger.LogWarning($"⚠️ Fallback by exception: {exception.Message}");
                        }
                        if (isEty)
                        {
                            _logger.LogWarning($"⚠️ Fallback by empty result");
                        }
                        if (oldcache != null)
                        {
                            _logger.LogInformation("✅ Fallback: возвращаю старые данные из кэша");
                            return oldcache;
                        }
                        if (_memorycache.TryGetValue(stalekey, out string stalecached))
                        {
                            _logger.LogInformation($"✅ Returning stale copy for {stalecached}");
                            return stalecached;
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Fallback: кэш пуст, возвращаю default");
                            return default;
                        }
                    },onFallbackAsync: async (outcome, ctx) =>
                    {
                        _logger.LogError($"🆘 Fallback сработал: {outcome.Exception?.Message}");
                        await Task.CompletedTask;
                    });

                var fallbackresult = await fallback.ExecuteAsync(async () =>
                {
                    var result = await SearchMethod(user, contactname).ConfigureAwait(false);

                    if (result != null)
                    {
                        var options = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                        _memorycache.Set(cache_key, result, options);

                        var staleoptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                        _memorycache.Set(stalekey, result, options);
                        return result;
                    }
                    return "";
                });
                return fallbackresult;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return "";
            }
            finally
            { 
                _semaphoreSlim.Release();
            }
        }

        public async Task<string> SearchMethod(string user, string contactname)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            string resultstring  = "";
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command = "SELECT COUNT(*) FROM ContactUserBD WHERE Name = @N AND UserName = @U";

                await using (var commandsql = new SQLiteCommand(command, connection, transaction))
                {
                    commandsql.Parameters.AddWithValue("@N", user);
                    commandsql.Parameters.AddWithValue("@U", contactname);

                    var result = await commandsql.ExecuteScalarAsync().ConfigureAwait(false);

                    int count = Convert.ToInt32(result);
                    if (count > 0)
                    {
                        resultstring = "Успешно";
                    }
                    await transaction.CommitAsync().ConfigureAwait(false);
                    return resultstring;
                };

            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (transaction.RollbackAsync() ?? Task.CompletedTask);
                return "";
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return "";
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }

        public async Task CreateIndex()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_ContactUserBD_IndexValidate ON ContactUserBD(Name,UserName)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
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

        public async Task<bool> IdexProverka()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_ContactUserBD_IndexValidate' AND tbl_name = 'ContactUserBD'";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        bool exec = Convert.ToInt32(result) == 1;

                        if (exec)
                        {
                            _logger.LogWarning("Индекс IX_ContactUserBD_IndexValidate создан");
                            return true;
                        }
                        return false;
                    }
                    return false;
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
