using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz_MongoDB.Data.Mongo;
using MongoDB.Driver;

namespace Labb3_Quiz.Data.Mongo.Repositories
{
    public class MongoCategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<CategoryDocument> _categoryCollection;

        public MongoCategoryRepository(MongoDbContext dbContext)
        {
            _categoryCollection = dbContext.Categories;
        }

        public Task<List<CategoryDocument>> GetAllAsync() =>
            _categoryCollection
                .Find(FilterDefinition<CategoryDocument>.Empty)
                .SortBy(c => c.Name)
                .ToListAsync();

        public async Task<CategoryDocument> CreateAsync(CategoryDocument category)
        {
            ArgumentNullException.ThrowIfNull(category);
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name cannot be empty.", nameof(category));

            var normalizedName = category.Name.Trim();

            // undvik dubbletter case-insensitive med normalizedName.ToLower()
            var existing = await _categoryCollection
                .Find(c => c.Name.ToLower() == normalizedName.ToLower())
                .FirstOrDefaultAsync();

            if (existing != null)
                return existing;

            category.Name = normalizedName;

            await _categoryCollection.InsertOneAsync(category);
            return category;
        }

        public Task UpdateAsync(CategoryDocument category)
        {
            ArgumentNullException.ThrowIfNull(category);
            if (string.IsNullOrWhiteSpace(category.Id))
                throw new ArgumentException("Category id cannot be empty.", nameof(category));
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException("Category name cannot be empty.", nameof(category));

            category.Name = category.Name.Trim();

            return _categoryCollection.ReplaceOneAsync(
                c => c.Id == category.Id,
                category);
        }

        public Task DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Category id cannot be empty.", nameof(id));

            return _categoryCollection.DeleteOneAsync(c => c.Id == id);
        }
    }
}