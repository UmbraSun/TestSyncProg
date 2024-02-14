using Newtonsoft.Json;
using SQLite;
using TestSyncProg.Common;
using TestSyncProg.DatabaseInstanceImitation;
using TestSyncProg.DbContexts;
using TestSyncProg.Entity;
using TestSyncProg.Helpers;

Console.WriteLine("Hello World");

//var seed = new List<EntityTrackerMSSql>
//{
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = JsonConvert.SerializeObject(new MaterialSqlite() { IsDeleted = false, LastUpdate = DateTime.UtcNow, Name = "Doska" })},
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = JsonConvert.SerializeObject(new MaterialSqlite() { IsDeleted = false, LastUpdate = DateTime.UtcNow, Name = "Gvozd" })},
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = JsonConvert.SerializeObject(new MaterialSqlite() { IsDeleted = false, LastUpdate = DateTime.UtcNow, Name = "Pesok" })},
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = JsonConvert.SerializeObject(new MaterialSqlite() { IsDeleted = false, LastUpdate = DateTime.UtcNow, Name = "Cement" })},
//};

//var seed = new List<EntityTrackerMSSql>
//{
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = $"'A', false, '{DateTime.UtcNow}'" },
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = $"'B', false, '{DateTime.UtcNow}'" },
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = $"'C', false, '{DateTime.UtcNow}'" },
//    new EntityTrackerMSSql(){ TableName = nameof(MaterialSqlite), CommandType = CommandTypeEnum.Add, ModelJson = $"'D', false, '{DateTime.UtcNow}'" },
//};

//await contextMSSql.AddRangeAsync(seed);
//await contextMSSql.SaveChangesAsync();

var first = new FirstBaseInstance();
var second = new SecondBaseInstance();
while (true) ;

