using Messangers.Delegate;
using Messangers.SQLite.DbPath;
using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Sqlite
{
    public partial class PoolSQLite
    {
        private readonly Stack<SQLiteConnection> _available = new Stack<SQLiteConnection>();
        private readonly List<SQLiteConnection> _inUse = new List<SQLiteConnection>();
        private readonly object _lock = new object();
        private readonly string _dbpath;
        private readonly int _maxCouhnt = 10;
        private readonly ILogger<PoolSQLite> _loggr;
        private readonly ExceptionDelegate _exceptionDelegate;
     
        public PoolSQLite(ILogger<PoolSQLite> loggr, ExceptionDelegate exceptionDelegate)
        {
            DbPathClass dbpath = new DbPathClass();
            _dbpath = dbpath.dbpath();
            _loggr = loggr;
            _exceptionDelegate = exceptionDelegate;
        }

        public SQLiteConnection CreateConnection()
        {
            try
            {
                SQLiteConnection connection = new SQLiteConnection($"Data Source={_dbpath}");
                connection.Open();
                return connection;
            }
            catch (SQLiteException ex)
            {
                _loggr.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                throw;
            }
        }

        public SQLiteConnection ConnctionOpen()
        {
            try
            {
                lock (_lock)
                {
                    SQLiteConnection connection = null;
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
                    if (_inUse.Count < _maxCouhnt)
                    {
                        connection = CreateConnection();
                    }
                    else
                        
                    {throw new Exception("пулл занят");
                    }
                    _inUse.Add(connection);
                    return connection;
                }
            }
            catch (SQLiteException ex)
            {
                _loggr.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                throw;
            }
            catch (Exception ex)
            {
                _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                throw;
            }
        }

        public void ConnectionClose(SQLiteConnection connection)
        {
            try 
            {
                lock (_lock)
                {
                    if (_inUse.Contains(connection))
                    {
                        _inUse.Remove(connection);

                        if (connection != null)
                        {
                            if (connection.State == System.Data.ConnectionState.Open)
                            {
                                _available.Push(connection);
                            }
                            else
                            {
                                connection.Dispose();
                            }
                        }
                    }
                    else
                    {
                        _loggr.LogError("Соединение не найдено");
                    }
                }
            }
            catch (SQLiteException ex)
            {
                _loggr.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
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
