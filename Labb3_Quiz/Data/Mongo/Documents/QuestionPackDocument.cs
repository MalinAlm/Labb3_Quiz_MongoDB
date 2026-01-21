using Labb3_Quiz.Models;
using Labb3_Quiz.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Labb3_Quiz.Data.Mongo.Documents
{
    internal class QuestionPackDocument
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Difficulty Difficulty { get; set; }
        public int TimeLimitInSeconds { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? CategoryId { get; set; }
        public List<QuestionDocument> Questions { get; set; } = new();

    }
}
