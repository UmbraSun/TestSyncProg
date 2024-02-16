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
            var guid = Guid.NewGuid();
            var uniqId = $"{guid.ToString("D")} {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff")}";
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
                int k = 0;
                do
                {
                    var randomId = new Random().Next(default, (_context.Materials.Query().LastOrDefault()?.Id ?? default) + 1);
                    model = _context.Materials.Query().FirstOrDefault(x => x.Id == randomId);
                    if (++k > 100)
                        break;
                }
                while (model is null);
                if (++k > 100)
                    continue;
                model.Name = model.Name + " edited";
                await EditModelInBases(model);
            }
        }

        private async Task AddModelToBases(MaterialSqlite model)
        {
            try
            {
                connection.BeginTransaction();
                if (!_context.Materials.TryInsert(model))
                    throw new Exception("Can't add in local db");

                connection.Commit();

                connection.BeginTransaction();
                var syncResponce = await TryToAddToMSSqlServer(_context.Materials.Query().Where(x => x.ServerId == null).Take(20).ToArray());
                foreach (var responce in syncResponce.ResponceToAddingModels)
                    DeserializeHelper.UpdateEntityServerIdByName(_context, responce.TableName, responce.LocalId, responce.ServerId);

                connection.Commit();
            }
            catch (Exception ex)
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

            responce.ResponceToAddingModels.Add(new ResponceToAddingModel
            {
                TableName = nameof(MaterialSqlite),
                LocalId = model.Id,
                ServerId = addMaterialResponce.Entity.Id
            });
        }

        private async Task EditModelInBases(MaterialSqlite model)
        {
            try
            {
                connection.BeginTransaction();
                if (!_context.Materials.TryUpdate(model))
                    throw new Exception();
                connection.Commit();

                var updateEntity = _msSqlContextSet.Materials.FirstOrDefault(x => x.Id == model.ServerId);
                updateEntity.Name = model.Name;
                updateEntity.LastUpdate = model.LastUpdate;
                updateEntity.IsDeleted = model.IsDeleted;
                _msSqlContextSet.Materials.Update(updateEntity);
                await _msSqlContextSet.SaveChangesAsync();

                await _msSqlContextSet.EntityTracker.AddAsync(new EntityTrackerMSSql
                {
                    ServerId = updateEntity.Id,
                    LocalId = model.Id,
                    CommandType = CommandTypeEnum.Edit,
                    ModelJson = JsonConvert.SerializeObject(model),
                    TableName = nameof(MaterialSqlite),
                    UniqueUserId = _baseName
                });
                await _msSqlContextSet.SaveChangesAsync();
            }
            catch
            {
                connection.Rollback();
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
                catch(Exception ex)
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
                    connection.Insert(DeserializeHelper.GetAnEntityOfSqlite( _context, entity.ModelJson, entity.TableName, entity.CommandType));
                    break;
                case CommandTypeEnum.Edit:
                    connection.Update(DeserializeHelper.GetAnEntityOfSqlite(_context, entity.ModelJson, entity.TableName, entity.CommandType));
                    break;
                default:
                    break;
            }
        }
    }
}
