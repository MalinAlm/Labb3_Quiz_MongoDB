using Labb3_Quiz.Data.Mongo.Documents;


namespace Labb3_Quiz.Data.Mongo.Repositories
{
    public interface IQuestionPackRepository
    {
        Task<List<QuestionPackDocument>> GetAllAsync();
        Task<QuestionPackDocument> CreateAsync(QuestionPackDocument questionPack);
        Task UpdateAsync(QuestionPackDocument questionPack);
        Task DeleteAsync(string id);
    }
}
