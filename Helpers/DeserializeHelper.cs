using Newtonsoft.Json;
using TestSyncProg.Common;
using TestSyncProg.DbContexts;
using TestSyncProg.Entity;

namespace TestSyncProg.Helpers
{
    public static class DeserializeHelper
    {
        public static object GetAnEntityOfSqlite(SqLiteDbContext context,
            string jsonModel, string modelType, CommandTypeEnum commandType)
        {
            switch (modelType)
            {
                case nameof(MaterialSqlite):
                    var model = JsonConvert.DeserializeObject<MaterialSqlite>(jsonModel);
                    if (commandType == CommandTypeEnum.Edit)
                    {
                        var entity = context.Materials.Query().FirstOrDefault(x => x.ServerId == model.ServerId);
                        if (entity is null)
                            return default;
                        model.Id = entity.Id;
                    }
                    return model;
                default:
                    return null;
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
