// File: Utilities/PackFingerprint.cs
// Purpose: Compute fingerprint ONLY from question content.
// Excludes: Name, Difficulty, Category, TimeLimit
// Swenglish comments, readable variable names 👌

using System.Security.Cryptography;
using System.Text;
using Labb3_Quiz.Models;

namespace Labb3_Quiz.Utilities
{
    public static class PackFingerprint
    {
        // ===== Model variant =====
        public static string ComputeQuestionsOnlyHash(QuestionPack questionPack)
        {
            ArgumentNullException.ThrowIfNull(questionPack);

            var stringBuilder = new StringBuilder(capacity: 1024);

            // OBS: Question order + count is part of fingerprint by design
            // (Add/remove/reorder -> different hash -> invalidate runs)
            var questions = questionPack.Questions ?? new List<Question>();

            for (int questionIndex = 0; questionIndex < questions.Count; questionIndex++)
            {
                var question = questions[questionIndex] ?? new Question();

                stringBuilder.Append("Question#")
                             .Append(questionIndex)
                             .Append('|');

                stringBuilder.Append("Query=")
                             .Append(Normalize(question.Query))
                             .Append('|');

                stringBuilder.Append("CorrectAnswer=")
                             .Append(Normalize(question.CorrectAnswer))
                             .Append('|');

                // ALWAYS 3 incorrect answers – order matters
                // If array is null or wrong length, we still produce a stable output (defensive)
                var incorrectAnswers = question.IncorrectAnswers ?? Array.Empty<string>();

                for (int answerIndex = 0; answerIndex < 3; answerIndex++)
                {
                    var incorrect = answerIndex < incorrectAnswers.Length
                        ? incorrectAnswers[answerIndex]
                        : string.Empty;

                    stringBuilder.Append("IncorrectAnswer")
                                 .Append(answerIndex)
                                 .Append('=')
                                 .Append(Normalize(incorrect))
                                 .Append('|');
                }

                stringBuilder.AppendLine(); // delimiter per question
            }

            return ComputeSha256Hex(stringBuilder.ToString());
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Trim + normalize newlines so Windows/Linux doesn't cause hash diffs
            return value.Trim()
                        .Replace("\r\n", "\n")
                        .Replace("\r", "\n");
        }

        private static string ComputeSha256Hex(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(inputBytes);

            var hexStringBuilder = new StringBuilder(capacity: hashBytes.Length * 2);
            foreach (var hashByte in hashBytes)
            {
                hexStringBuilder.Append(hashByte.ToString("x2"));
            }

            return hexStringBuilder.ToString();
        }
    }
}
