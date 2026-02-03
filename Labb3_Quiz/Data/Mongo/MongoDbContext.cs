// File: Data/Mongo/MongoDbContext.cs
// (Namespace is already correct for your solution structure.)

using Labb3_Quiz.Data.Mongo.Documents;
using MongoDB.Driver;

namespace Labb3_Quiz_MongoDB.Data.Mongo
{
    /// <summary>
    /// Central MongoDB context.
    /// Responsible for exposing typed collections used by the application.
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(MongoSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ArgumentException("MongoDB connection string is missing.", nameof(settings));

            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
                throw new ArgumentException("MongoDB database name is missing.", nameof(settings));

            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        // ===== Collections =====
        // Note: MongoDB will create collections on first insert if they don't exist yet.

        public IMongoCollection<QuestionPackDocument> QuestionPacks =>
            _database.GetCollection<QuestionPackDocument>("QuestionPacks");

        public IMongoCollection<CategoryDocument> Categories =>
            _database.GetCollection<CategoryDocument>("Categories");

        /// <summary>
        /// Stores completed quiz runs (one document per playthrough).
        /// Used for Top5, statistics, and run invalidation.
        /// </summary>
        public IMongoCollection<QuizRunDocument> QuizRuns =>
            _database.GetCollection<QuizRunDocument>("QuizRuns");
    }
}
