namespace TestSyncProg.Interfaces
{
    public interface ILocalEntity : IHasId, IHasServerId
    {
        bool IsUpdatedLocal { get; set; }
    }
}
