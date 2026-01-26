using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_Quiz.Data.Mongo.Documents
{
    public class CategoryDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("Name")]
        public string Name { get; set; }

    }
}
