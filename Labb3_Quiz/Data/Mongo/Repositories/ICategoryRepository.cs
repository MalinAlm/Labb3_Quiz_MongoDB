using Labb3_Quiz.Data.Mongo.Documents;

namespace Labb3_Quiz.Data.Mongo.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<CategoryDocument>> GetAllAsync();
        Task<CategoryDocument> CreateAsync(CategoryDocument category);
        Task UpdateAsync(CategoryDocument category);
        Task DeleteAsync(string id);
    }
}
