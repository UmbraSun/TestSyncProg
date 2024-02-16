namespace TestSyncProg.DTO
{
    public class SyncResponceDto
    {
        public List<ResponceToAddingModel> ResponceToAddingModels { get; set; } = new List<ResponceToAddingModel>();

        public long? LastUpdatedEntityTrackerId { get; set; }
    }
}
