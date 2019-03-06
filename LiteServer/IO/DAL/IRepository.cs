namespace LiteServer.IO.DAL.Repository
{
    public interface IRepository<TEntitry, TId>
    {
        TEntitry Select(TId id);
        void Insert(TEntitry entity);
        void Delete(TId id);
    }
}
