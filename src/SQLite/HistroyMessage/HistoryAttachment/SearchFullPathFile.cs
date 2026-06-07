using Messangers.Controllers.ControlleruDLOAFfiLES;
using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;

namespace Messangers.SQLite.HistroyMessage.HistoryAttachment
{
    public class SearchFullPathFile
    {
        private readonly ILogger<SearchFullPathFile> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private bool _ischeckindex = false;

        public SearchFullPathFile(ILogger<SearchFullPathFile> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;

            Task.Run(async () => await Inithializate());
        }

        public async Task Inithializate()
        {
            if (_ischeckindex) return;

            if (_ischeckindex == false)
            {
                await IndexCreate();
                await IndexProverka();
            }
            
            _ischeckindex = true;
        }

        public async Task<List<Fullpath>> SearchRequest(int id)
        { 
            SQLiteConnection connection = null;
            List<Fullpath> listpath = new List<Fullpath>();
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT FilePath, FileName, MimeType FROM Attachments WHERE Id = @Id";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@Id", id);

                    await using (var path = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (path != null)
                        {
                            var filepath = path.GetOrdinal("FilePath");
                            var filename = path.GetOrdinal("filename");
                            var mimetype = path.GetOrdinal("MimeType");

                            while (await path.ReadAsync().ConfigureAwait(false))
                            {
                                listpath.Add(new Fullpath
                                {
                                    FilePath = path.IsDBNull(filepath) ? "" : path.GetString(filepath),
                                    FileName = path.IsDBNull(filename) ? "" : path.GetString(filename),
                                    MimeType = path.IsDBNull(mimetype) ? "" : path.GetString(mimetype)
                                });       
                            }
                            return listpath;
                        }
                        else
                        {
                            _logger.LogError("Не удалось найти полный путь для скачивания файла");
                            return new List<Fullpath>();
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                return new List<Fullpath>();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new List<Fullpath>();
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }

        public async Task IndexCreate()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_AttachmentDowloadFull ON Attachment(Id)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogWarning("Индекс IX_AttachmentDowloadFull  Создан");
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

        public async Task<bool> IndexProverka()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_AttachmentDowloadFull' AND tbl_name = 'Attachment'";

                await using (var sqlitecommand = new SQLiteCommand(command, connection))
                {
                    var result = await sqlitecommand.ExecuteScalarAsync().ConfigureAwait(false);

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
