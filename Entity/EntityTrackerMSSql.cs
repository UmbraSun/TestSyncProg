using TestSyncProg.Common;

namespace TestSyncProg.Entity;

public class EntityTrackerMSSql
{
    public long Id { get; set; }

    public string TableName { get; set; }

    public CommandTypeEnum CommandType { get; set; }

    public string ModelJson { get; set; }
}
