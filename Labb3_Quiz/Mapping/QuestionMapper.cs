using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Models;

namespace Labb3_Quiz.Mapping
{
    public static class QuestionMapper
    {
        public static Question MongoDBToQuestion(this QuestionDocument doc)
        {
            var incorrect = doc.IncorrectAnswers ?? Array.Empty<string>();

            var incorrectAnswer1 = incorrect.Length > 0 ? incorrect[0] : string.Empty;
            var incorrectAnswer2 = incorrect.Length > 1 ? incorrect[1] : string.Empty;
            var incorrectAnswer3 = incorrect.Length > 2 ? incorrect[2] : string.Empty;

            return new Question(
                doc.Query ?? string.Empty,
                doc.CorrectAnswer ?? string.Empty,
                incorrectAnswer1,
                incorrectAnswer2,
                incorrectAnswer3);
        }



        public static QuestionDocument QuestionToMongoDB(this Question model)
        {
            return new QuestionDocument
            {
                Query = model.Query,
                CorrectAnswer = model.CorrectAnswer,
                IncorrectAnswers = model.IncorrectAnswers
            };
        }

    }
}
