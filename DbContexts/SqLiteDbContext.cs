using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLite;
using TestSyncProg.Entity;

namespace TestSyncProg.DbContexts
{
    public class SqLiteDbContext : IDisposable
    {
        public static readonly SqLiteDbContext _instance = new SqLiteDbContext();
        public readonly SQLiteConnection _connection;
        private const string _databaseFileName = "TestSqlite.db";
        private const SQLiteOpenFlags _flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
        private string _databasePath
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _databaseFileName);

        private bool isDisposed;
        private object _lock = new object();

        private SqLiteDbContext()
        {
            _connection = new SQLiteConnection(_databasePath, _flags);
            ConfigureDataSets();
        }

        public SqliteDataSet<MaterialSqlite> Materials { get; set; }

        public SqliteDataSet<ConfigsSqLite> Configs { get; set; }

        private void ConfigureDataSets()
        {
            Materials = new SqliteDataSet<MaterialSqlite>(_connection);
            Configs = new SqliteDataSet<ConfigsSqLite>(_connection);
        }

        ~SqLiteDbContext() => Dispose(false);

        public int ExecuteScalar(string sql, params object[] args)
        {
            lock (_lock)
            {
                return _connection.ExecuteScalar<int>(sql, args);
            }
        }

        public int Execute(string sql, params object[] args)
        {
            lock (_lock)
            {
                return _connection.Execute(sql, args);
            }
        }

        public T Single<T>(string sql, params object[] args) where T : new()
            => Query<T>(sql, args).Single();

        public IEnumerable<T> Query<T>(string sql, params object[] args) where T : new()
        {
            lock (_lock)
            {
                return _connection.DeferredQuery<T>(sql, args);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                _connection.Close();
                isDisposed = true;
            }
        }
    }
}
