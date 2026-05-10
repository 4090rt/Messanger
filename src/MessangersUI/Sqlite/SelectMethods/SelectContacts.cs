using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.DataModel;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Sqlite.SelectMethods
{
    public class SelectContacts
    {
        private ILogger<SelectContacts> _logger;
        private PoolSQLite _poosqlite;
        private IMemoryCache _memoryCache;
        public SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private bool _Is_chekedindex = false;
        public SelectContacts(ILogger<SelectContacts> logger, PoolSQLite poosqlite, IMemoryCache memoryCache)
        {
            _logger = logger;
            _poosqlite = poosqlite;
            _memoryCache = memoryCache;

            Task.Run(async () => await Initihializate()).ConfigureAwait(false);
        }

        public async Task Initihializate()
        {
            if (_Is_chekedindex == true) return;

            if (_Is_chekedindex == false)
            {
                await CreateINdex();
                await ProverkaIndex();
            }

            _Is_chekedindex = true;
        }

        public void ClearCache()
        {
            string cache_key = "cachekey_contacts";
            _memoryCache.Remove(cache_key);
            _logger.LogInformation("Кэш контактов очищен");
        }

        public async Task<List<UserContact>> CacheRequest(string user)
        {
            string cache_key = "cachekey_contacts";
            string stalecachekey = $"stale{cache_key}";
            List<UserContact> oldcache = null;

            if (_memoryCache.TryGetValue(cache_key, out List<UserContact> cached))
            { 
                oldcache = cached;
                return cached;
            }

            await _semaphoreSlim.WaitAsync();

            try
            {
                if (_memoryCache.TryGetValue(cache_key, out List<UserContact> cached2))
                {
                    return cached2;
                }

                var fallback = Policy<List<UserContact>>
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
                        if (_memoryCache.TryGetValue(stalecachekey, out List<UserContact> syalecached))
                        {
                            _logger.LogInformation($"✅ Returning stale copy");
                            return syalecached;
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
                    var result = await Request(user).ConfigureAwait(false);

                    if (result != null)
                    {
                        var options = new MemoryCacheEntryOptions()
                         .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                         .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                        _memoryCache.Set(cache_key, result, options);

                        var staleoptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                        _memoryCache.Set(cache_key, result, staleoptions);

                        return result;
                    }
                    else
                    {
                        _logger.LogWarning("Результат пуст");
                        return default;
                    }
                });
                return fallbackresult;
            }
            catch (SqliteException ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло sql исключение" + ex.Message + ex.StackTrace);
                return new List<UserContact>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new List<UserContact>();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<List<UserContact>> Request(string user)
        {
           string username;
           string photo;
           SQLiteConnection sQLiteConnection = null;
            List<UserContact> list = new List<UserContact>();
            try
            { 
                sQLiteConnection = _poosqlite.ConnctionOpen();

                string command = "SELECT Login, PHOTO FROM ContactsBase WHERE User = @U";

                await using (var sqlcommand = new SQLiteCommand(command, sQLiteConnection))
                {
                    sqlcommand.Parameters.AddWithValue("@U", user);
                   var result =  await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false);

                    while (await result.ReadAsync())
                    { 
                        username = result.GetString(0);
                        photo = result.GetString(1);

                        var newlist = new UserContact
                        {
                            Username = username,
                            photo = photo
                        };

                        list.Add(newlist);
                    }
                    return list;
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло sql исключение" + ex.Message + ex.StackTrace);
                return new List<UserContact>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return new List<UserContact>();
            }
            finally
            {
                if (sQLiteConnection != null)
                {
                    _poosqlite.ConnectionClose(sQLiteConnection);
                }
            }
        }

        public async Task CreateINdex()
        {
            SQLiteConnection sQLiteConnection = null;
            try
            {
                sQLiteConnection = _poosqlite.ConnctionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_ContactsBase_Index ON ContactsBase(User)";

                await using (var sqlcomand = new SQLiteCommand(command, sQLiteConnection))
                { 
                    await sqlcomand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogWarning("Индекс IX_ContactsBase_Index Создан!");
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло sql исключение" + ex.Message + ex.StackTrace);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
            }
            finally
            {
                if (sQLiteConnection != null)
                {
                    _poosqlite.ConnectionClose(sQLiteConnection);
                }
            }
        }

        public async Task<bool> ProverkaIndex()
        {
            SQLiteConnection sQLiteConnection = null;
            try
            {
                sQLiteConnection = _poosqlite.ConnctionOpen();

                string command = "SELECT COUNT(*)  FROM sqlite_master WHERE type = 'index' AND name = 'IX_ContactsBase_Index' AND tbl_name = 'ContactsBase'";

                await using (var sqlcommand = new SQLiteCommand(command, sQLiteConnection))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        bool exec = Convert.ToInt32(result) == 1;
                        if (exec)
                        {
                            _logger.LogWarning("Индекс IX_ContactsBase_Index существует");
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning("Индекс IX_ContactsBase_Index  не существует");
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
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло sql исключение" + ex.Message + ex.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return false;
            }
            finally
            {
                if (sQLiteConnection != null)
                {
                    _poosqlite.ConnectionClose(sQLiteConnection);
                }
            }
        }
    }
}
