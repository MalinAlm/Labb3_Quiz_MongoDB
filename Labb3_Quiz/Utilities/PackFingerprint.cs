// File: Utilities/PackFingerprint.cs
// Purpose: Compute fingerprint ONLY from question content.
// Excludes: Name, Difficulty, Category, TimeLimit
// Swenglish comments, readable variable names 👌

using System.Security.Cryptography;
using System.Text;
using Labb3_Quiz.Models;
using Labb3_Quiz.ViewModels;

namespace Labb3_Quiz.Utilities
{
    public static class PackFingerprint
    {
        // ===== Model variant =====
        public static string ComputeQuestionsOnlyHash(QuestionPack questionPack)
        {
            if (questionPack == null)
                throw new ArgumentNullException(nameof(questionPack));

            var stringBuilder = new StringBuilder(capacity: 1024);

            // OBS: Question order + count is part of fingerprint by design
            for (int questionIndex = 0; questionIndex < questionPack.Questions.Count; questionIndex++)
            {
                var question = questionPack.Questions[questionIndex];

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
                for (int answerIndex = 0; answerIndex < question.IncorrectAnswers.Length; answerIndex++)
                {
                    stringBuilder.Append("IncorrectAnswer")
                                 .Append(answerIndex)
                                 .Append('=')
                                 .Append(Normalize(question.IncorrectAnswers[answerIndex]))
                                 .Append('|');
                }

                stringBuilder.AppendLine(); // delimiter per question
            }

            return ComputeSha256Hex(stringBuilder.ToString());
        }

        // ===== ViewModel convenience overload =====
        public static string ComputeQuestionsOnlyHash(QuestionPackViewModel questionPackViewModel)
        {
            if (questionPackViewModel == null)
                throw new ArgumentNullException(nameof(questionPackViewModel));

            // IMPORTANT: caller must ensure ViewModel is synced to Model
            return ComputeQuestionsOnlyHash(questionPackViewModel.Model);
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim()
                        .Replace("\r\n", "\n")
                        .Replace("\r", "\n");
        }

        private static string ComputeSha256Hex(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(inputBytes);

            var hexStringBuilder = new StringBuilder(hashBytes.Length * 2);
            foreach (var hashByte in hashBytes)
            {
                hexStringBuilder.Append(hashByte.ToString("x2"));
            }

            return hexStringBuilder.ToString();
        }
    }
}
