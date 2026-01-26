using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Models;


namespace Labb3_Quiz.Mapping
{
    public static class QuestionPackMapper
    {

        public static QuestionPack MongoDBToQuestionPack(this QuestionPackDocument doc)
        {
            var model = new QuestionPack(doc.Name, doc.Difficulty, doc.TimeLimitInSeconds)
            {
                Id = doc.Id,
                CategoryName = doc.CategoryName
            };

            if (doc.Questions != null)
            {
                model.Questions = doc.Questions
                    .Select(qd => qd.MongoDBToQuestion())
                    .ToList();
            }

            return model;
        }


        public static QuestionPackDocument QuestionPackToMongoDB(this QuestionPack model)
        {
            return new QuestionPackDocument
            {
                Id = string.IsNullOrWhiteSpace(model.Id) ? null : model.Id,
                Name = model.Name,
                Difficulty = model.Difficulty,
                TimeLimitInSeconds = model.TimeLimitInSeconds,
                CategoryName = model.CategoryName,
                Questions = model.Questions?.Select(q => q.QuestionToMongoDB()).ToList() ?? new()
            };
        }


    }
}
