using Newtonsoft.Json;
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
