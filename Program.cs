using Newtonsoft.Json;
using SQLite;
using TestSyncProg.Common;
using TestSyncProg.DbContexts;
using TestSyncProg.Entity;
using TestSyncProg.Helpers;

Console.WriteLine("Hello World");

var seed = new List<EntityTrackerMSSql>
{
    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = JsonConvert.SerializeObject(new MaterialSqlite() { IsDeleted = false, LastUpdate = DateTime.UtcNow, Name = "Doska" })},
    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = JsonConvert.SerializeObject(new MaterialSqlite() { IsDeleted = false, LastUpdate = DateTime.UtcNow, Name = "Gvozd" })},
    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = JsonConvert.SerializeObject(new MaterialSqlite() { IsDeleted = false, LastUpdate = DateTime.UtcNow, Name = "Pesok" })},
    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = JsonConvert.SerializeObject(new MaterialSqlite() { IsDeleted = false, LastUpdate = DateTime.UtcNow, Name = "Cement" })},
};

//var seed = new List<EntityTrackerMSSql>
//{
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = $"'A', false, '{DateTime.UtcNow}'" },
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = $"'B', false, '{DateTime.UtcNow}'" },
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = $"'C', false, '{DateTime.UtcNow}'" },
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = $"'D', false, '{DateTime.UtcNow}'" },
//};

var contextSqlite = SqLiteDbContext._instance;
var contextMSSql = new MSSqlDbContext();
await contextMSSql.AddRangeAsync(seed);
await contextMSSql.SaveChangesAsync();

while (true)
{
    await Task.Delay(2000);
    var last = contextSqlite.Configs.Query().LastOrDefault();
    var lastId = last == null ? -1 : last.LastUpdatedServerId;
    var entities = contextMSSql.EntityTracker.Where(x => x.Id > lastId).ToList();

    var connection = contextSqlite._connection;

    try
    {
        connection.BeginTransaction();
        foreach (var entity in entities)
            ExecuteSqliteCommnd(connection, contextSqlite, entity);
        var config = contextSqlite.Configs.Query().LastOrDefault();
        if (config != null)
        {
            config.LastUpdatedServerId = entities.LastOrDefault()?.Id ?? config.LastUpdatedServerId;
            contextSqlite.Configs.TryUpdate(config);
        }
        else
        {
            contextSqlite.Configs.TryInsert(new ConfigsSqLite { LastUpdatedServerId = entities.LastOrDefault()?.Id ?? -1 });
        }
        connection.Commit();
    }
    catch
    {
        connection.Rollback();
    }
}

void ExecuteSqliteCommnd(SQLiteConnection connection, SqLiteDbContext context,
    EntityTrackerMSSql entity)
{
    switch (entity.CommandType)
    {
        case CommandTypeEnum.Add:
            connection.Insert(DeserializeHelper.GetAnEntityOfTheDesiredType(entity.ModelJson, entity.TableName));
            break;
        default:
            break;
    }
}