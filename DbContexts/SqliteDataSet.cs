using System.Linq.Expressions;
using SQLite;

namespace TestSyncProg.DbContexts
{
    public class SqliteDataSet<T> where T : class, new()
    {
        protected SQLiteConnection _connection;
        private object _locker = new object();

        public SqliteDataSet(SQLiteConnection connection)
        {
            _connection = connection;
            _connection.CreateTable<T>();
            SeedData();
            ConfigureTable();
        }

        public T ExecuteScalar<T>(string query, params string[] parameters)
            => _connection.ExecuteScalar<T>(query, parameters);

        public int Execute(string query, params string[] parameters)
            => _connection.Execute(query, parameters);

        public TableQuery<T> Query()
            => _connection.Table<T>();

        public TableQuery<T> Query(Expression<Func<T, bool>> predicate)
            => _connection.Table<T>().Where(predicate);

        public bool TryUpdate(T entity)
            => TryDoAction(_connection.Update, entity);

        public bool TryInsert(T entity)
            => TryDoAction(_connection.Insert, entity);

        public bool TryDelete(T entity)
            => TryDoAction(_connection.Delete, entity);

        public int TryDeleteAll()
            => _connection.DeleteAll<T>();


        private bool TryDoAction(Func<object, int> func, T entity)
        {
            lock (_locker)
            {
                bool result = false;
                try
                {
                    func?.Invoke(entity);
                    result = true;
                    return result;
                }
                catch (SQLiteException ex) { }
                catch (Exception ex) { }
                return result;
            }
        }

        protected virtual void SeedData()
        {

        }

        protected virtual void ConfigureTable()
        {

        }
    }
}
