using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz_MongoDB.Data.Mongo;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb3_Quiz.Data.Mongo.Repositories
{
    internal class MongoCategoryRepository
    {
        private readonly IMongoCollection<CategoryDocument> _categories;

        public MongoCategoryRepository(MongoDbContext context)
        {
            _categories = context.Categories;
        }

        public async Task<List<CategoryDocument>> GetAllAsync()
        {
            return await _categories
                .Find(FilterDefinition<CategoryDocument>.Empty)
                .ToListAsync();
        }

        public async Task<CategoryDocument> CreateAsync(CategoryDocument category)
        {
            await _categories.InsertOneAsync(category);
            return category;
        }

        public async Task UpdateAsync(CategoryDocument category)
        {
            await _categories.ReplaceOneAsync(
                c => c.Id == category.Id,
                category
            );
        }

        public async Task DeleteAsync(string id)
        {
            await _categories.DeleteOneAsync(c => c.Id == id);
        }
    }
}

