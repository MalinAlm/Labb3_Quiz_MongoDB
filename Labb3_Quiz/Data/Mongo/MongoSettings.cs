// File: Data/Mongo/MongoSettings.cs
// Purpose: Holds MongoDB connection settings (connection string + database name)

namespace Labb3_Quiz_MongoDB.Data.Mongo
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}