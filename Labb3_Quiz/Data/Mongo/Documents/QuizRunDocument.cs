using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Labb3_Quiz.Data.Mongo.Documents
{

    [BsonIgnoreExtraElements]
    public class QuizRunDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }


        [BsonRepresentation(BsonType.ObjectId)]
        public string PackId { get; set; } = string.Empty;

        public string PlayerName { get; set; } = string.Empty;

        public DateTime CreatedUtc { get; set; }

        public int TotalTimeSeconds { get; set; }

        public int CorrectCount { get; set; }

        public List<QuizRunAnswerDocument> Answers { get; set; } = new();
    }


    public class QuizRunAnswerDocument
    {
        public int QuestionIndexInPack { get; set; }

        public string ChosenAnswerText { get; set; } = string.Empty;
    }
}
