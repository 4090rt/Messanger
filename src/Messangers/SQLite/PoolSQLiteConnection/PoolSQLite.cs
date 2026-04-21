using Messangers.Delegate;
using Messangers.SQLite.DbPath;
using MessangersUI.Delegate;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Messangers.SQLite.PoolSQLiteConnection
{
    public class PoolSQLite
    {
        private readonly Stack<SQLiteConnection> _available = new Stack<SQLiteConnection>();
        private readonly List<SQLiteConnection> _inUse = new List<SQLiteConnection>();
        private readonly object _lock = new object();
        private readonly string _dbpath;
        private readonly int _maxCouhnt = 10;
        private readonly ILogger _loggr;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;

        public PoolSQLite(ILogger loggr, ExceptionDelegate exceptionDelegate, SQLiteExceptionDelegate sQLiteExceptionDelegate)
        {
            DbPathClass dbpath = new DbPathClass();
            _dbpath = dbpath.dbpath();
            _loggr = loggr;
            _exceptionDelegate = exceptionDelegate;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
        }

        public SQLiteConnection CreateConnection()
        {
            try
            {
                var connection = new SQLiteConnection($"Data Source={_dbpath}");
                connection.Open();
                return connection;
            }
            catch (SQLiteException ex)
            {
                _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                throw;
            }
            catch (Exception ex)
            {
                _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                throw;
            }
        }

        public SQLiteConnection ConnectionOpen()
        {
            try
            {
                lock (_lock)
                {
                    SQLiteConnection connection;

                    if (_available.Count > 0)
                    {
                        connection = _available.Pop();

                        if (_available == null)
                        {
                            connection = CreateConnection();
                        }

                        if (connection.State != System.Data.ConnectionState.Open)
                        {
                            connection = CreateConnection();
                        }
                    }
                    else if (_inUse.Count < _maxCouhnt)
                    {
                        connection = CreateConnection();
                    }
                    else
                    {
                        throw new Exception("Пулл занят");
                    }
                    _inUse.Add(connection);
                    return connection;
                }
            }
            catch (SQLiteException ex)
            {
                _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                throw;
            }
            catch (Exception ex)
            {
                _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                throw;
            }
        }

        public void CloseConnection(SQLiteConnection connection)
        {
            try
            {
                lock (_lock)
                {
                    if (_inUse.Contains(connection))
                    {
                        _inUse.Remove(connection);

                        if (connection.State == System.Data.ConnectionState.Open)
                        {
                            _available.Push(connection);
                        }
                        else
                        {
                            connection.Dispose();
                        }
                    }
                    else
                    {
                        throw new Exception("Соединение не найдено");
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                throw;
            }
            catch (Exception ex)
            {
                _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                throw;
            }
        }
    }
}
