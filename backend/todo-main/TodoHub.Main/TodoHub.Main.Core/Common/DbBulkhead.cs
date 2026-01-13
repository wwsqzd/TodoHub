namespace TodoHub.Main.Core.Common
{
    public sealed class DbBulkhead : Bulkhead
    {
        public DbBulkhead() : base(name: "db", MaxConcurrency: 50) { }
    }
}
