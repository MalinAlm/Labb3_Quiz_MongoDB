using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Labb3_Quiz.Data.Mongo.Documents
{
    /// <summary>
    /// Represents one completed quiz run for a specific QuestionPack.
    /// Stored as a separate collection for VG requirements.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class QuizRunDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Stable reference to the QuestionPack (ObjectId stored as string).
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string PackId { get; set; } = string.Empty;

        /// <summary>
        /// Player name as entered before starting the quiz.
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the run was completed.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Total time spent answering questions (seconds).
        /// </summary>
        public int TotalTimeSeconds { get; set; }

        /// <summary>
        /// Number of correctly answered questions.
        /// </summary>
        public int CorrectCount { get; set; }

        /// <summary>
        /// Answers given by the player, indexed by original question order in the pack.
        /// This makes answer statistics stable even when UI answer order is shuffled.
        /// </summary>
        public List<QuizRunAnswerDocument> Answers { get; set; } = new();
    }

    /// <summary>
    /// One answer chosen by the player for a specific question.
    /// </summary>
    public class QuizRunAnswerDocument
    {
        /// <summary>
        /// Index of the question in the QuestionPack at the time of play.
        /// </summary>
        public int QuestionIndexInPack { get; set; }

        /// <summary>
        /// The exact answer text the player selected.
        /// </summary>
        public string ChosenAnswerText { get; set; } = string.Empty;
    }
}
