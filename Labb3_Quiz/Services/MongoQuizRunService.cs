
using Labb3_Quiz.Data.Mongo.Repositories;
using Labb3_Quiz.Models;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.Design.Behavior;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

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

        //store the completed run
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
                normalizedPlayerName = "Anonymous"; // fail-safe (UI should prevent empty anyways...)

            if (totalTimeSeconds < 0)
                totalTimeSeconds = 0;

            if (correctCount < 0)
                correctCount = 0;

            answers ??= new List<QuizRunAnswerDto>();

            // Def-cleanup: keep empty string for timeout 
            // but valid and notNull
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

        public Task<List<Top5EntryDto>> GetTop5Async(string packId)
        {
            if (string.IsNullOrWhiteSpace(packId))
                return Task.FromResult(new List<Top5EntryDto>());

            return _quizRunRepository.GetTop5Async(packId, topN: 5);
        }

        //TODO – GetAnswerCountsAsync
        // identity                                                                             [V]
        // handle tiomeouts                                                                     [V]
        // Support for Repository  (all answers / pack + Qindex)                                 [V]
        // Efficiency for polish                                                                [V]
        // Make sure error safe (emtpy dictionary if DB gives trouble)                          [V]
        // UI completeness – Ensure all 4 answer options appear with count 0 if never chosen.   [V]

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

    // One chosen answer by the player for one question (stored by stable question index in pack)
    public sealed record QuizRunAnswerDto(int QuestionIndexInPack, string ChosenAnswerText);

    // Leaderboard top 5 record
    public sealed record Top5EntryDto(string PlayerName, int CorrectCount, int TotalTimeSeconds, DateTime CreatedUtc);

}
