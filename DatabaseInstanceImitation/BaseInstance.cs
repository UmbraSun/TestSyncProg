using Newtonsoft.Json;
using SQLite;
using TestSyncProg.Common;
using TestSyncProg.DbContexts;
using TestSyncProg.Entity;
using TestSyncProg.Helpers;

namespace TestSyncProg.DatabaseInstanceImitation
{
    public class BaseInstance
    {
        private readonly string _baseName;
        protected readonly SqLiteDbContext _context;
        protected readonly MSSqlDbContext _msSqlContextGet = new MSSqlDbContext();
        protected readonly MSSqlDbContext _msSqlContextSet = new MSSqlDbContext();
        protected SQLiteConnection connection => _context._connection;

        public BaseInstance(string baseName)
        {
            _baseName = baseName;
            _context = new SqLiteDbContext(baseName);
            StartTasks();
        }

        public void StartTasks()
        {
            Task.Run(GetSyncData);
            Task.Run(SendSyncData);
        }


        // -------------- Send data

        protected virtual async void SendSyncData()
        {
            int i = default;
            while (true)
            {
                await Task.Delay(9000);

                var model = new MaterialSqlite
                {
                    IsDeleted = false,
                    LastUpdate = DateTime.UtcNow,
                    Name = $"material {++i}"
                };

                var id = _context.LocalEntityTracer.Query().LastOrDefault()?.Id;
                try
                {
                    connection.BeginTransaction();
                    var tracerModel = new EntityTrackerMSSql
                    {
                        Id = ++id ?? 1,
                        CommandType = CommandTypeEnum.Add,
                        TableName = nameof(MaterialSqlite),
                        ModelJson = JsonConvert.SerializeObject(model),
                        UniqueUserId = _baseName
                    };
                    _context.Materials.TryInsert(model);
                    _context.LocalEntityTracer.TryInsert(tracerModel);

                    var localIdConfig = _context.Configs.Query().FirstOrDefault(x => x.ConfigType == ConfigType.LastLocalId);
                    var localId = localIdConfig?.Value ?? 0;
                    var lastSavedLocalId = await TryToAddToMSSqlServer(_context.LocalEntityTracer.Query().Where(x => x.Id > localId).Take(20).ToArray());
                    if (localIdConfig is null)
                        _context.Configs.TryInsert(new ConfigsSqLite { ConfigType = ConfigType.LastLocalId, Value = lastSavedLocalId });
                    else
                    {
                        localIdConfig.Value = lastSavedLocalId;
                        var a = _context.Configs.TryUpdate(localIdConfig);
                    }

                    connection.Commit();
                }
                catch
                {
                    connection.Rollback();
                }
            }
        }

        public async Task<long> TryToAddToMSSqlServer(params EntityTrackerMSSql[] entities)
        {
            using (var tr = _msSqlContextSet.Database.BeginTransaction())
            {
                long lastId = entities.FirstOrDefault()?.Id ?? 0;
                try
                {
                    foreach (var item in entities)
                    {
                        var check = _msSqlContextSet.EntityTracker.Any(x => x.LocalId == item.Id
                            && x.UniqueUserId == item.UniqueUserId);
                        if (check)
                            continue;

                        item.LocalId = item.Id;
                        item.Id = default;
                        await ExecuteMsSqlCommand(item);
                        await _msSqlContextSet.EntityTracker.AddAsync(item);
                        await _msSqlContextSet.SaveChangesAsync();
                        lastId = item.LocalId;
                    }

                    await tr.CommitAsync();
                    return lastId;
                }
                catch
                {
                    await tr.RollbackAsync();
                    return entities.FirstOrDefault()?.Id ?? 0;
                }
            }
        }

        private async Task ExecuteMsSqlCommand(EntityTrackerMSSql entity)
        {
            switch (entity.CommandType)
            {
                case CommandTypeEnum.Add:
                    await _msSqlContextSet.AddAsync(DeserializeHelper.GetAnEntityOfMsSql(entity.ModelJson, entity.TableName));
                    break;
                default:
                    break;
            }
        }


        // ------------- Get data

        protected async virtual void GetSyncData()
        {
            while (true)
            {
                await Task.Delay(2000);
                var last = _context.Configs.Query().FirstOrDefault(x => x.ConfigType == ConfigType.LastServerId);
                var lastId = last == null ? -1 : last.Value;
                var entities = _msSqlContextGet.EntityTracker.Where(x => x.Id > lastId
                    && x.UniqueUserId != _baseName).Take(20).ToList();

                try
                {
                    connection.BeginTransaction();
                    foreach (var entity in entities)
                        if (!_context.LocalEntityTracer.Query().Any(x => x.Id == entity.LocalId 
                            && x.UniqueUserId != _baseName))
                            ExecuteSqliteCommnd(entity);

                    if (last != null)
                    {
                        last.Value = entities.LastOrDefault()?.Id ?? last.Value;
                        _context.Configs.TryUpdate(last);
                    }
                    else
                    {
                        _context.Configs.TryInsert(new ConfigsSqLite
                        {
                            ConfigType = ConfigType.LastServerId,
                            Value = entities.LastOrDefault()?.Id ?? -1
                        });
                    }
                    connection.Commit();
                }
                catch
                {
                    connection.Rollback();
                }
            }
        }

        private void ExecuteSqliteCommnd(EntityTrackerMSSql entity)
        {
            switch (entity.CommandType)
            {
                case CommandTypeEnum.Add:
                    connection.Insert(DeserializeHelper.GetAnEntityOfSqlite(entity.ModelJson, entity.TableName));
                    break;
                default:
                    break;
            }
        }
    }
}
