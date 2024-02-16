using Newtonsoft.Json;
using SQLite;
using TestSyncProg.Common;
using TestSyncProg.DbContexts;
using TestSyncProg.DTO;
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
                    Name = $"material {++i}",
                    ServerId = null
                };

                await AddModelToBases(model);

                //var randomId = new Random().Next(default, _context.Materials.Query().LastOrDefault()?.Id ?? default);

                //do
                //{
                //    model = _context.Materials.Query().FirstOrDefault(x => x.Id == randomId);
                //}
                //while (model is null);
                //model.Name = model.Name + " edited";
                //EditModelInBases(id, model);
            }
        }

        private async Task AddModelToBases(MaterialSqlite model)
        {
            try
            {
                connection.BeginTransaction();
                if (!_context.Materials.TryInsert(model))
                    throw new Exception("Can't to add in local db");

                var localIdConfig = _context.Configs.Query().FirstOrDefault(x => x.ConfigType == ConfigType.LastLocalId);
                var localId = localIdConfig?.Value ?? 0;
                connection.Commit();

                connection.BeginTransaction();
                var syncResponce = await TryToAddToMSSqlServer(_context.Materials.Query().Where(x => x.ServerId == null).Take(20).ToArray());
                foreach (var responce in syncResponce.ResponceToAddingModels)
                    DeserializeHelper.UpdateEntityServerIdByName(_context, responce.TableName, responce.LocalId, responce.ServerId);

                if (localIdConfig is null)
                    _context.Configs.TryInsert(new ConfigsSqLite { ConfigType = ConfigType.LastLocalId, Value = syncResponce.LastUpdatedEntityTrackerId ?? default });
                else
                {
                    if (syncResponce.LastUpdatedEntityTrackerId.HasValue)
                    {
                        localIdConfig.Value = syncResponce.LastUpdatedEntityTrackerId.Value;
                        _context.Configs.TryUpdate(localIdConfig);
                    }
                }
                connection.Commit();
            }
            catch (Exception ex)
            {
                connection.Rollback();
            }
        }

        private void EditModelInBases(long? localTracerLastId, MaterialSqlite model)
        {
            try
            {
                connection.BeginTransaction();
                var tracerModel = new EntityTrackerMSSql
                {
                    Id = ++localTracerLastId ?? 1,
                    CommandType = CommandTypeEnum.Edit,
                    TableName = nameof(MaterialSqlite),
                    ModelJson = JsonConvert.SerializeObject(model),
                    UniqueUserId = _baseName
                };
                if (!_context.Materials.TryUpdate(model))
                    return;
                connection.Commit();
            }
            catch
            {
                connection.Rollback();
            }
        }

        public async Task<SyncResponceDto> TryToAddToMSSqlServer(params MaterialSqlite[] entities)
        {
            var responce = new SyncResponceDto();
            foreach (var item in entities)
                await AddToMsSql(responce, item);

            return responce;
        }

        private async Task AddToMsSql(SyncResponceDto responce, MaterialSqlite model)
        {
            var check = _msSqlContextSet.EntityTracker.FirstOrDefault(x => x.LocalId == model.Id && x.UniqueUserId == _baseName);
            if (check != null)
            {
                responce.LastUpdatedEntityTrackerId = check.Id;
                responce.ResponceToAddingModels.Add(new ResponceToAddingModel
                {
                    TableName = nameof(MaterialSqlite),
                    LocalId = model.Id,
                    ServerId = (int)check.ServerId
                });
            }


            var addMaterialResponce = await _msSqlContextSet.Materials.AddAsync(new MaterialMSSql
            {
                IsDeleted = model.IsDeleted,
                LastUpdate = model.LastUpdate,
                Name = model.Name
            });
            await _msSqlContextSet.SaveChangesAsync();

            model.ServerId = addMaterialResponce.Entity.Id;
            var addTrackerResponce = await _msSqlContextSet.EntityTracker.AddAsync(new EntityTrackerMSSql
            {
                CommandType = CommandTypeEnum.Add,
                LocalId = model.Id,
                ServerId = addMaterialResponce.Entity.Id,
                ModelJson = JsonConvert.SerializeObject(model),
                TableName = nameof(MaterialSqlite),
                UniqueUserId = _baseName
            });
            await _msSqlContextSet.SaveChangesAsync();

            responce.LastUpdatedEntityTrackerId = addTrackerResponce.Entity.Id;
            responce.ResponceToAddingModels.Add(new ResponceToAddingModel
            {
                TableName = nameof(MaterialSqlite),
                LocalId = model.Id,
                ServerId = addMaterialResponce.Entity.Id
            });
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
                case CommandTypeEnum.Edit:
                    connection.Update(DeserializeHelper.GetAnEntityOfSqlite(entity.ModelJson, entity.TableName));
                    break;
                default:
                    break;
            }
        }
    }
}
