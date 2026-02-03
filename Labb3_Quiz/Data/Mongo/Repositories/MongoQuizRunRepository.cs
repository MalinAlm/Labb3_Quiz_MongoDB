// File: Data/Mongo/Repositories/MongoQuizRunRepository.cs
// Mongo repository for QuizRuns (VG): insert completed runs, Top5, per-question answer stats, and deletions.

using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Services;
using Labb3_Quiz_MongoDB.Data.Mongo;
using MongoDB.Driver;

namespace Labb3_Quiz.Data.Mongo.Repositories
{
    public class MongoQuizRunRepository : IQuizRunRepository
    {
        private readonly IMongoCollection<QuizRunDocument> _quizRuns;

        public MongoQuizRunRepository(MongoDbContext mongoDbContext)
        {
            ArgumentNullException.ThrowIfNull(mongoDbContext);

            // MongoDbContext must expose:
            // public IMongoCollection<QuizRunDocument> QuizRuns { get; }
            _quizRuns = mongoDbContext.QuizRuns;
        }

        public async Task<bool> AnyRunsByPackIdAsync(string packId)
        {
            if (string.IsNullOrWhiteSpace(packId))
                return false;

            // Faster than CountDocumentsAsync for "any?"
            return await _quizRuns
                .Find(run => run.PackId == packId)
                .Limit(1)
                .AnyAsync();
        }

        public Task DeleteRunsByPackIdAsync(string packId)
        {
            if (string.IsNullOrWhiteSpace(packId))
                return Task.CompletedTask;

            return _quizRuns.DeleteManyAsync(run => run.PackId == packId);
        }

        public async Task InsertCompletedRunAsync(
            string packId,
            string playerName,
            DateTime createdUtc,
            int totalTimeSeconds,
            int correctCount,
            List<QuizRunAnswerDto> answers)
        {
            if (string.IsNullOrWhiteSpace(packId))
                throw new ArgumentException("Pack id cannot be empty.", nameof(packId));

            var normalizedPlayerName = (playerName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedPlayerName))
                normalizedPlayerName = "Anonymous";

            if (totalTimeSeconds < 0) totalTimeSeconds = 0;
            if (correctCount < 0) correctCount = 0;

            answers ??= new List<QuizRunAnswerDto>();

            var document = new QuizRunDocument
            {
                PackId = packId,
                PlayerName = normalizedPlayerName,
                CreatedUtc = createdUtc,
                TotalTimeSeconds = totalTimeSeconds,
                CorrectCount = correctCount,
                Answers = answers
                    .Where(answer => answer.QuestionIndexInPack >= 0)
                    .Select(answer => new QuizRunAnswerDocument
                    {
                        QuestionIndexInPack = answer.QuestionIndexInPack,
                        ChosenAnswerText = answer.ChosenAnswerText ?? string.Empty
                    })
                    .ToList()
            };

            await _quizRuns.InsertOneAsync(document);
        }

        public async Task<List<Top5EntryDto>> GetTop5Async(string packId, int topN)
        {
            if (string.IsNullOrWhiteSpace(packId) || topN <= 0)
                return new List<Top5EntryDto>();

            // Sort: Most correct first, then best (lowest) time, then earliest completion
            var topRuns = await _quizRuns
                .Find(run => run.PackId == packId)
                .SortByDescending(run => run.CorrectCount)
                .ThenBy(run => run.TotalTimeSeconds)
                .ThenBy(run => run.CreatedUtc)
                .Limit(topN)
                .ToListAsync();

            return topRuns
                .Select(run => new Top5EntryDto(
                    PlayerName: run.PlayerName,
                    CorrectCount: run.CorrectCount,
                    TotalTimeSeconds: run.TotalTimeSeconds,
                    CreatedUtc: run.CreatedUtc))
                .ToList();
        }

        public async Task<List<string>> GetChosenAnswersForQuestionAsync(string packId, int questionIndexInPack)
        {
            if (string.IsNullOrWhiteSpace(packId) || questionIndexInPack < 0)
                return new List<string>();

            // Server-side filter:
            // Only return runs that have at least one matching answer entry.
            var filter = Builders<QuizRunDocument>.Filter.And(
                Builders<QuizRunDocument>.Filter.Eq(run => run.PackId, packId),
                Builders<QuizRunDocument>.Filter.ElemMatch(
                    run => run.Answers,
                    answer => answer.QuestionIndexInPack == questionIndexInPack
                )
            );

            // Only fetch Answers (projection)
            var projectedAnswers = await _quizRuns
                .Find(filter)
                .Project(run => run.Answers)
                .ToListAsync();

            var chosenAnswers = new List<string>();

            foreach (var answers in projectedAnswers)
            {
                if (answers == null) continue;

                foreach (var answer in answers)
                {
                    if (answer.QuestionIndexInPack != questionIndexInPack) continue;
                    chosenAnswers.Add(answer.ChosenAnswerText ?? string.Empty);
                }
            }

            return chosenAnswers;
        }
    }
}
