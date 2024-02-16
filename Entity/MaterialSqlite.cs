using SQLite;

namespace TestSyncProg.Entity;

public class MaterialSqlite
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Unique]
    public int? ServerId { get; set; }

    public string Name { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime LastUpdate { get; set; }
}
