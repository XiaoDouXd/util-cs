namespace IdGen
{
    public static class IdGen<TGen, TId>
        where TId : struct
        where TGen : IIdGen<TId>, new()
    {
        private static readonly Lazy<IIdGen<TId>> Inst = new(() => new TGen());
        public static TId Gen() => Inst.Value.Gen();
    }

    public interface IIdGen<out TId> where TId : struct
    {
        public TId Gen();
    }
}