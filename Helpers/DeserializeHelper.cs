using Newtonsoft.Json;
using TestSyncProg.DbContexts;
using TestSyncProg.Entity;

namespace TestSyncProg.Helpers
{
    public static class DeserializeHelper
    {
        public static object GetAnEntityOfSqlite(string jsonModel, string modelType)
        {
            switch (modelType)
            {
                case nameof(MaterialSqlite):
                    return JsonConvert.DeserializeObject<MaterialSqlite>(jsonModel);
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
