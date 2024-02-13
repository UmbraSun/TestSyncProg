using Newtonsoft.Json;
using TestSyncProg.Entity;

namespace TestSyncProg.Helpers
{
    public static class DeserializeHelper
    {
        public static object GetAnEntityOfTheDesiredType(string jsonModel, string modelType)
        {
            switch (modelType)
            {
                case nameof(MaterialSqlite):
                    return JsonConvert.DeserializeObject<MaterialSqlite>(jsonModel);
                default:
                    return null;
            }
        }
    }
}
