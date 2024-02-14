using TestSyncProg.DbContexts;

namespace TestSyncProg.DatabaseInstanceImitation
{
    public class SecondBaseInstance : BaseInstance
    {
        public SecondBaseInstance() : base("TestHB2.db")
        { }
    }
}
