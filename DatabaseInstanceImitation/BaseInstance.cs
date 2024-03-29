﻿using System;
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
        private readonly string _uniqueUserName;
        protected readonly SqLiteDbContext _contextGet;
        protected readonly SqLiteDbContext _contextSet;
        protected readonly MSSqlDbContext _msSqlContextGet = new MSSqlDbContext();
        protected readonly MSSqlDbContext _msSqlContextSet = new MSSqlDbContext();
        protected SQLiteConnection connectionGet => _contextGet._connection;
        protected SQLiteConnection connectionSet => _contextSet._connection;

        public BaseInstance(string baseName)
        {
            //var guid = Guid.NewGuid();
            //_uniqueUserName = $"{guid.ToString("D")} {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff")}";
            _uniqueUserName = baseName;
            _contextGet = new SqLiteDbContext(baseName);
            _contextSet = new SqLiteDbContext(baseName);
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

                #region Add to bases
                var model = new MaterialSqlite
                {
                    IsUpdatedLocal = false,
                    LastUpdate = DateTime.UtcNow,
                    Name = $"material {++i}",
                    ServerId = null
                };
                await AddModelToBases(model);
                #endregion

                #region Edit model
                int k = default;
                do
                {
                    var randomId = new Random().Next(default, (_contextSet.Materials.Query().LastOrDefault()?.Id ?? default) + 1);
                    model = _contextSet.Materials.Query().FirstOrDefault(x => x.Id == randomId && !x.IsDeletedLocal);
                    if (++k > 100)
                        break;
                }
                while (model is null);
                if (++k > 100)
                    continue;
                model.Name = $"{model.Name} edited";
                if(model.ServerId != null)
                    model.IsUpdatedLocal = true;
                await EditModelInBases(model);
                #endregion

                #region Delete model
                int l = new Random().Next(0, 7);
                if (l > 3)
                {
                    do
                    {
                        var randomId = new Random().Next(default, (_contextSet.Materials.Query().LastOrDefault()?.Id ?? default) + 1);
                        model = _contextSet.Materials.Query().FirstOrDefault(x => x.Id == randomId && !x.IsUpdatedLocal);
                        if (++k > 100)
                            break;
                    }
                    while (model is null);
                    if (++k > 100)
                        continue;

                    model.IsDeletedLocal = true;
                    await DeleteModelInBase(model);
                }
                #endregion
            }
        }

        private async Task DeleteModelInBase(MaterialSqlite model)
        {
            try
            {
                connectionSet.BeginTransaction();
                if (!_contextSet.Materials.TryUpdate(model))
                    throw new Exception("Can't delete in local db");

                connectionSet.Commit();

                connectionSet.BeginTransaction();
                var syncResponce = await DeleteMSSqlServerModel(_contextSet.Materials.Query().Where(x => x.IsDeletedLocal).Take(20).ToArray());
                foreach (var responce in syncResponce.ResponceModels)
                    DeserializeHelper.DeleteEntityByServerId(_contextSet, responce);

                connectionSet.Commit();
            }
            catch (Exception ex)
            {
                connectionSet.Rollback();
            }
        }

        private async Task<SyncResponceDto> DeleteMSSqlServerModel(params MaterialSqlite[] entities)
        {
            var responce = new SyncResponceDto();
            foreach (var item in entities)
                await DeteleInMssSql(responce, item);

            return responce;
        }

        private async Task DeteleInMssSql(SyncResponceDto responce, MaterialSqlite model)
        {
            var check = _msSqlContextSet.EntityTracker.FirstOrDefault(x => x.LocalId == model.Id && x.UniqueUserId == _uniqueUserName 
                && x.CommandType == CommandTypeEnum.Delete);
            if (check != null)
            {
                responce.ResponceModels.Add(new ResponceModel
                {
                    TableName = nameof(MaterialSqlite),
                    LocalId = model.Id,
                    ServerId = (int)check.ServerId
                });
                return;
            }

            var entity = _msSqlContextSet.Materials.FirstOrDefault(x => x.Id == model.ServerId);
            if (entity is null)
                return;

            entity.DeleteDate = DateTime.UtcNow; 
            var addMaterialResponce = _msSqlContextSet.Materials.Update(entity);
            await _msSqlContextSet.SaveChangesAsync();

            var addTrackerResponce = await _msSqlContextSet.EntityTracker.AddAsync(new EntityTrackerMSSql
            {
                CommandType = CommandTypeEnum.Delete,
                LocalId = model.Id,
                ServerId = addMaterialResponce.Entity.Id,
                ModelJson = JsonConvert.SerializeObject(model),
                TableName = nameof(MaterialSqlite),
                UniqueUserId = _uniqueUserName
            });
            await _msSqlContextSet.SaveChangesAsync();

            responce.ResponceModels.Add(new ResponceModel
            {
                TableName = nameof(MaterialSqlite),
                LocalId = model.Id,
                ServerId = addMaterialResponce.Entity.Id
            });
        }

        private async Task AddModelToBases(MaterialSqlite model)
        {
            try
            {
                connectionSet.BeginTransaction();
                if (!_contextSet.Materials.TryInsert(model))
                    throw new Exception("Can't add in local db");

                connectionSet.Commit();

                connectionSet.BeginTransaction();
                var syncResponce = await TryToAddToMSSqlServer(_contextSet.Materials.Query().Where(x => x.ServerId == null).Take(20).ToArray());
                foreach (var responce in syncResponce.ResponceModels)
                    DeserializeHelper.UpdateEntityServerIdByName(_contextSet, responce);

                connectionSet.Commit();
            }
            catch (Exception ex)
            {
                connectionSet.Rollback();
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
            var check = _msSqlContextSet.EntityTracker.FirstOrDefault(x => x.LocalId == model.Id && x.UniqueUserId == _uniqueUserName);
            if (check != null)
            {
                responce.ResponceModels.Add(new ResponceModel
                {
                    TableName = nameof(MaterialSqlite),
                    LocalId = model.Id,
                    ServerId = (int)check.ServerId
                });
                return;
            }

            var addMaterialResponce = await _msSqlContextSet.Materials.AddAsync(new MaterialMSSql
            {
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
                UniqueUserId = _uniqueUserName
            });
            await _msSqlContextSet.SaveChangesAsync();

            responce.ResponceModels.Add(new ResponceModel
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
                connectionSet.BeginTransaction();
                if (!_contextSet.Materials.TryUpdate(model))
                    throw new Exception();
                connectionSet.Commit();

                var updatedLocalEntities = _contextSet.Materials.Query().Where(x => x.IsUpdatedLocal == true);
                foreach (var updatedLocal in updatedLocalEntities)
                {
                    var updateEntity = _msSqlContextSet.Materials.FirstOrDefault(x => x.Id == updatedLocal.ServerId);
                    if (updateEntity is null)
                        continue;

                    updateEntity.Name = updatedLocal.Name;
                    updateEntity.LastUpdate = updatedLocal.LastUpdate;
                    _msSqlContextSet.Materials.Update(updateEntity);
                    await _msSqlContextSet.SaveChangesAsync();

                    await _msSqlContextSet.EntityTracker.AddAsync(new EntityTrackerMSSql
                    {
                        ServerId = updateEntity.Id,
                        LocalId = updatedLocal.Id,
                        CommandType = CommandTypeEnum.Edit,
                        ModelJson = JsonConvert.SerializeObject(updatedLocal),
                        TableName = nameof(MaterialSqlite),
                        UniqueUserId = _uniqueUserName
                    });
                    await _msSqlContextSet.SaveChangesAsync();
                }
            }
            catch(Exception ex)
            {
                connectionSet.Rollback();
            }
        }



        // ------------- Get data

        protected async virtual void GetSyncData()
        {
            while (true)
            {
                await Task.Delay(2000);
                var last = _contextGet.Configs.Query().FirstOrDefault(x => x.ConfigType == ConfigType.LastServerId);
                var lastId = last == null ? -1 : last.Value;
                var entities = _msSqlContextGet.EntityTracker.Where(x => x.Id > lastId
                    && (x.UniqueUserId != _uniqueUserName || x.CommandType != CommandTypeEnum.Add)).Take(20).ToList();

                try
                {
                    connectionGet.BeginTransaction();
                    long? lastUpdatedId = default;
                    foreach (var entity in entities)
                        lastUpdatedId = ExecuteSqliteCommnd(entity);

                    if (last != null)
                    {
                        last.Value = lastUpdatedId ?? last.Value;
                        _contextGet.Configs.TryUpdate(last);
                    }
                    else
                    {
                        _contextGet.Configs.TryInsert(new ConfigsSqLite
                        {
                            ConfigType = ConfigType.LastServerId,
                            Value = entities.LastOrDefault()?.Id ?? -1
                        });
                    }
                    connectionGet.Commit();
                }
                catch (Exception ex)
                {
                    connectionGet.Rollback();
                }
            }
        }

        private long? ExecuteSqliteCommnd(EntityTrackerMSSql entity)
        {
            try
            {
                switch (entity.CommandType)
                {
                    case CommandTypeEnum.Add:
                        connectionGet.Insert(DeserializeHelper.GetSqliteEntity(_contextGet, entity.ModelJson, entity.TableName, entity.CommandType));
                        break;
                    case CommandTypeEnum.Edit:
                        connectionGet.Update(DeserializeHelper.GetSqliteEntity(_contextGet, entity.ModelJson, entity.TableName, entity.CommandType));
                        break;
                    case CommandTypeEnum.Delete:
                        var model = DeserializeHelper.GetSqliteEntity(_contextGet, entity.ModelJson, entity.TableName, entity.CommandType);
                        if(model != null)
                            connectionGet.Delete(model);
                        break;
                }
                return entity.Id;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
