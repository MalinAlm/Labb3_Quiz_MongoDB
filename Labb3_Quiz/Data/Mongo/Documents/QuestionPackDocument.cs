using Labb3_Quiz.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Labb3_Quiz.Data.Mongo.Documents
{
    [BsonIgnoreExtraElements]
    public class QuestionPackDocument
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null!;

        public string Name { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Difficulty Difficulty { get; set; }
        public int TimeLimitInSeconds { get; set; }

        [BsonElement("CategoryName")]
        public string? CategoryName { get; set; }
        public List<QuestionDocument> Questions { get; set; } = new();

    }
}
