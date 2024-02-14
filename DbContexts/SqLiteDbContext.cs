using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLite;
using TestSyncProg.Entity;

namespace TestSyncProg.DbContexts
{
    public class SqLiteDbContext : IDisposable
    {
        public readonly SQLiteConnection _connection;
        private const SQLiteOpenFlags _flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
        private string DatabasePath(string databaseFileName) 
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseFileName);

        private bool isDisposed;
        private object _lock = new object();

        public SqLiteDbContext(string databaseFileName)
        {
            var path = DatabasePath(databaseFileName);
            _connection = new SQLiteConnection(path, _flags);
            ConfigureDataSets();
        }

        public SqliteDataSet<MaterialSqlite> Materials { get; set; }

        public SqliteDataSet<EntityTrackerMSSql> LocalEntityTracer { get; set; }

        public SqliteDataSet<ConfigsSqLite> Configs { get; set; }

        private void ConfigureDataSets()
        {
            Configs = new SqliteDataSet<ConfigsSqLite>(_connection);
            Materials = new SqliteDataSet<MaterialSqlite>(_connection);
            LocalEntityTracer = new SqliteDataSet<EntityTrackerMSSql>(_connection);
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
