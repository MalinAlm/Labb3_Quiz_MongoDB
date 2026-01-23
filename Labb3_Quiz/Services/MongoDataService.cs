using Labb3_Quiz.Data.Mongo.Repositories;
using Labb3_Quiz.Mapping;
using Labb3_Quiz.Models;

namespace Labb3_Quiz.Services
{
    public class MongoQuizDataService
    {
        private readonly IQuestionPackRepository _questionPackRepository;

        public MongoQuizDataService(IQuestionPackRepository questionPackRepository)
        {
            _questionPackRepository = questionPackRepository;
        }

        public async Task<List<QuestionPack>> LoadPacksAsync()
        {
            var docs = await _questionPackRepository.GetAllAsync();
            return docs.Select(d => d.MongoDBToQuestionPack()).ToList();
        }

        public async Task UpsertPackAsync(QuestionPack pack)
        {
            var doc = pack.QuestionPackToMongoDB();

            if (string.IsNullOrWhiteSpace(doc.Id))
            {
                var created = await _questionPackRepository.CreateAsync(doc);
                pack.Id = created.Id; // viktigt!
            }
            else
            {
                await _questionPackRepository.UpdateAsync(doc);
            }
        }

        public Task DeletePackAsync(string id)
            => _questionPackRepository.DeleteAsync(id);
    }
}
