namespace TodoHub.Main.Core.Common
{
    public sealed class EsBulkhead : Bulkhead
    {
        public EsBulkhead() : base(name: "es", MaxConcurrency: 10) { }
    }
}
