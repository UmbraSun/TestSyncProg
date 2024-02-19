using Newtonsoft.Json;
using TestSyncProg.Common;
using TestSyncProg.DbContexts;
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
                    //if (commandType == CommandTypeEnum.Edit)
                    //{
                    //    var entity = context.Materials.Query().FirstOrDefault(x => x.ServerId == model.ServerId);
                    //    if (entity is null)
                    //        return default;
                    //    model.Id = entity.Id;
                    //    model.IsUpdatedLocal = false;
                    //}
                    //else if(commandType == CommandTypeEnum.Delete)
                    //{
                    //    model = context.Materials.Query().FirstOrDefault(x => x.ServerId == model.ServerId);
                    //}
                    //return model;
                default:
                    return default;
            }
        }

        public static void UpdateEntityServerIdByName(SqLiteDbContext context, string modelType, int localId, int serverId)
        {
            switch (modelType)
            {
                case nameof(MaterialSqlite):
                    var model = context.Materials.Query().FirstOrDefault(x => x.Id == localId);
                    if (model is null)
                        return;
                    model.ServerId = serverId;
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
