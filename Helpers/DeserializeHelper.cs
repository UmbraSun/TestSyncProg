using Newtonsoft.Json;
using TestSyncProg.Common;
using TestSyncProg.DbContexts;
using TestSyncProg.DTO;
using TestSyncProg.Entity;
using TestSyncProg.Interfaces;

namespace TestSyncProg.Helpers
{
    public static class DeserializeHelper
    {
        public static object GetModifyEntityByCommandType(SqLiteDbContext context, ILocalEntity model, CommandTypeEnum commandType)
        {
            switch (commandType)
            {
                case CommandTypeEnum.Add:
                    return model;
                case CommandTypeEnum.Edit:
                    var entity = context.Materials.Query().FirstOrDefault(x => x.ServerId == model.ServerId);
                    if (entity is null)
                        return default;
                    model.Id = entity.Id;
                    model.IsUpdatedLocal = false;
                    return model;
                case CommandTypeEnum.Delete:
                    return context.Materials.Query().FirstOrDefault(x => x.ServerId == model.ServerId);
                default:
                    return default;
            }
        }

        public static object GetSqliteEntity(SqLiteDbContext context,
            string jsonModel, string modelType, CommandTypeEnum commandType)
        {
            switch (modelType)
            {
                case nameof(MaterialSqlite):
                    var model = JsonConvert.DeserializeObject<MaterialSqlite>(jsonModel);
                    return GetModifyEntityByCommandType(context, model, commandType);
                default:
                    return default;
            }
        }

        public static void DeleteEntityByServerId(SqLiteDbContext context, ResponceModel responce)
        {
            switch (responce.TableName)
            {
                case nameof(MaterialSqlite):
                    var model = context.Materials.Query().FirstOrDefault(x => x.ServerId == responce.ServerId);
                    if (model is null)
                        return;
                    context.Materials.TryDelete(model);
                    break;
            }
        }

        public static void UpdateEntityServerIdByName(SqLiteDbContext context, ResponceModel responce)
        {
            switch (responce.TableName)
            {
                case nameof(MaterialSqlite):
                    var model = context.Materials.Query().FirstOrDefault(x => x.Id == responce.LocalId);
                    if (model is null)
                        return;
                    model.ServerId = responce.ServerId;
                    context.Materials.TryUpdate(model);
                    break;
            }
        }

        public static object GetAnEntityOfMsSql(string jsonModel, string modelType)
        {
            switch (modelType)
            {
                case nameof(MaterialSqlite):
                    return JsonConvert.DeserializeObject<MaterialMSSql>(jsonModel);
                default:
                    return null;
            }
        }

    }
}
