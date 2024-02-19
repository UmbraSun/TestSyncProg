using SQLite;
using TestSyncProg.Interfaces;

namespace TestSyncProg.Entity;

public class MaterialSqlite : ILocalEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Unique]
    public int? ServerId { get; set; }

    public string Name { get; set; }

    public bool IsUpdatedLocal { get; set; }

    public DateTime LastUpdate { get; set; }
}
