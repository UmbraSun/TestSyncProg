using SQLite;
using TestSyncProg.Common;

namespace TestSyncProg.Entity;

public class ConfigsSqLite
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public ConfigType ConfigType { get; set; }

    public long Value { get; set; }
}
