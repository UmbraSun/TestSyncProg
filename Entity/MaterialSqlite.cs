using SQLite;

namespace TestSyncProg.Entity;

public class MaterialSqlite
{
    public string Name { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime LastUpdate { get; set; }
}
