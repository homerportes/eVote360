namespace eVote360.Domain.Interfaces
{
    public interface IGenericRepository<Entity> where Entity : class
    {
        #region Create / Update / Delete
        Task<Entity?> AddAsync(Entity entity);
        Task<List<Entity>?> AddRangeAsync(List<Entity> entities);
        Task<Entity?> UpdateAsync(int id, Entity entity);
        Task DeleteAsync(int id);
        #endregion

        #region Get methods
        Task<Entity?> GetById(int id);
        Task<List<Entity>> GetAllList();
        IQueryable<Entity> GetAllQuery();
        Task<List<Entity>> GetAllListWithInclude(List<string> properties);
        IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);
        #endregion
    }
}
