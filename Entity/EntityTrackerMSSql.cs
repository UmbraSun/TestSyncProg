using SQLite;
using TestSyncProg.Common;

namespace TestSyncProg.Entity;

public class EntityTrackerMSSql
{
    public long Id { get; set; }

    public string TableName { get; set; }

    public CommandTypeEnum CommandType { get; set; }

    public string ModelJson { get; set; }

    public string UniqueUserId { get; set; }

    public long LocalId { get; set; }

    public long ServerId { get; set; }
}
