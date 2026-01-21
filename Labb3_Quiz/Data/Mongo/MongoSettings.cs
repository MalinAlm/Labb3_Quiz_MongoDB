using MongoDB.Driver;
using Labb3_Quiz.Data.Mongo.Documents;

namespace Labb3_Quiz_MongoDB.Data.Mongo
{
    internal class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(MongoSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<QuestionPackDocument> QuestionPacks =>
            _database.GetCollection<QuestionPackDocument>("QuestionPacks");

        public IMongoCollection<CategoryDocument> Categories =>
            _database.GetCollection<CategoryDocument>("Categories");
    }
}
