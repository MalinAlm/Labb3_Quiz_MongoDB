// File: Data/Mongo/Repositories/IQuizRunRepository.cs
// Purpose: Repository contract for QuizRuns (VG).
// NOTE: Must be public + interface (not a class), so Services can depend on it.

using Labb3_Quiz.Services;

namespace Labb3_Quiz.Data.Mongo.Repositories
{
    public interface IQuizRunRepository
    {
        Task<bool> AnyRunsByPackIdAsync(string packId);

        Task DeleteRunsByPackIdAsync(string packId);

        Task InsertCompletedRunAsync(
            string packId,
            string playerName,
            DateTime createdUtc,
            int totalTimeSeconds,
            int correctCount,
            List<QuizRunAnswerDto> answers);

        Task<List<Top5EntryDto>> GetTop5Async(string packId, int topN);

        // Returns all chosen answers for a specific question index across all completed runs for that pack.
        Task<List<string>> GetChosenAnswersForQuestionAsync(string packId, int questionIndexInPack);
    }
}
