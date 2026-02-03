// File: Services/MongoQuizRunService.cs
// VG service: saves completed runs, Top5 leaderboard, per-answer statistics, and run deletion.
//
// NOTE:
// This service depends on an IQuizRunRepository abstraction.
// The Mongo implementation will live in Data.Mongo.Repositories and match this interface.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Labb3_Quiz.Data.Mongo.Repositories;

namespace Labb3_Quiz.Services
{
    public class MongoQuizRunService
    {
        private readonly IQuizRunRepository _quizRunRepository;

        public MongoQuizRunService(IQuizRunRepository quizRunRepository)
        {
            _quizRunRepository = quizRunRepository ?? throw new ArgumentNullException(nameof(quizRunRepository));
        }

        // Used by MainWindowViewModel (warnings / confirm invalidation flow)
        public Task<bool> AnyRunsByPackIdAsync(string packId)
        {
            if (string.IsNullOrWhiteSpace(packId))
                return Task.FromResult(false);

            return _quizRunRepository.AnyRunsByPackIdAsync(packId);
        }

        // Used when pack questions changed (fingerprint changed + confirmed) OR pack deleted
        public Task DeleteRunsByPackIdAsync(string packId)
        {
            if (string.IsNullOrWhiteSpace(packId))
                return Task.CompletedTask;

            return _quizRunRepository.DeleteRunsByPackIdAsync(packId);
        }

        // VG #1: store the completed run
        public Task SaveCompletedRunAsync(
            string packId,
            string playerName,
            int totalTimeSeconds,
            int correctCount,
            List<QuizRunAnswerDto> answers)
        {
            if (string.IsNullOrWhiteSpace(packId))
                throw new ArgumentException("Pack id cannot be empty.", nameof(packId));

            var normalizedPlayerName = (playerName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedPlayerName))
                normalizedPlayerName = "Anonymous"; // fail-safe (UI should prevent empty)

            if (totalTimeSeconds < 0)
                totalTimeSeconds = 0;

            if (correctCount < 0)
                correctCount = 0;

            answers ??= new List<QuizRunAnswerDto>();

            // Defensive cleanup: keep empty string for timeout if you want,
            // but ensure indices are valid and text isn't null.
            var cleanedAnswers = answers
                .Where(answer => answer.QuestionIndexInPack >= 0)
                .Select(answer => new QuizRunAnswerDto(
                    answer.QuestionIndexInPack,
                    answer.ChosenAnswerText ?? string.Empty))
                .ToList();

            return _quizRunRepository.InsertCompletedRunAsync(
                packId: packId,
                playerName: normalizedPlayerName,
                createdUtc: DateTime.UtcNow,
                totalTimeSeconds: totalTimeSeconds,
                correctCount: correctCount,
                answers: cleanedAnswers);
        }

        // VG #1: Top5 sorted by most correct, then fastest time, then earliest run
        public Task<List<Top5EntryDto>> GetTop5Async(string packId)
        {
            if (string.IsNullOrWhiteSpace(packId))
                return Task.FromResult(new List<Top5EntryDto>());

            return _quizRunRepository.GetTop5Async(packId, topN: 5);
        }

        // VG #2: for each answer option, how many previous players chose it.
        // Identity = answer text (shuffle does not matter).
        public async Task<Dictionary<string, int>> GetAnswerCountsAsync(string packId, int questionIndexInPack)
        {
            if (string.IsNullOrWhiteSpace(packId) || questionIndexInPack < 0)
                return new Dictionary<string, int>(StringComparer.Ordinal);

            var chosenAnswers = await _quizRunRepository.GetChosenAnswersForQuestionAsync(packId, questionIndexInPack);

            // Aggregate counts by exact answer text (Ordinal).
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var chosenAnswerText in chosenAnswers)
            {
                var key = chosenAnswerText ?? string.Empty;

                if (counts.TryGetValue(key, out var currentCount))
                    counts[key] = currentCount + 1;
                else
                    counts[key] = 1;
            }

            return counts;
        }
    }

    // ===== DTOs (used by PlayerViewModel + repository) =====

    // One chosen answer by the player for one question (stored by stable question index in pack)
    public sealed record QuizRunAnswerDto(int QuestionIndexInPack, string ChosenAnswerText);

    // Entry for leaderboard
    public sealed record Top5EntryDto(string PlayerName, int CorrectCount, int TotalTimeSeconds, DateTime CreatedUtc);

}
