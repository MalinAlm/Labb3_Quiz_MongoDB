using Labb3_Quiz.Data.Mongo.Documents;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Labb3_Quiz_MongoDB.Data.Mongo;

namespace Labb3_Quiz.Data.Mongo.Repositories
{
    public class MongoQuestionPackRepository : IQuestionPackRepository
    {
        private readonly IMongoCollection<QuestionPackDocument> _packs;

        public MongoQuestionPackRepository(MongoDbContext context)
        {
            _packs = context.QuestionPacks;
        }

        public async Task<List<QuestionPackDocument>> GetAllAsync()
        {
            return await _packs
                .Find(FilterDefinition<QuestionPackDocument>.Empty)
                .ToListAsync();
        }

        public async Task<QuestionPackDocument> CreateAsync(QuestionPackDocument pack)
        {
            await _packs.InsertOneAsync(pack);
            return pack;
        }

        public async Task UpdateAsync(QuestionPackDocument pack)
        {
            await _packs.ReplaceOneAsync(
                p => p.Id == pack.Id,
                pack
            );
        }

        public async Task DeleteAsync(string id)
        {
            await _packs.DeleteOneAsync(p => p.Id == id);
        }

      
    }
}
