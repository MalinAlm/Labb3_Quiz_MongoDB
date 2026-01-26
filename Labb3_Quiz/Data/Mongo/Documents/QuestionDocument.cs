

namespace Labb3_Quiz.Data.Mongo.Documents
{
    public class QuestionDocument
    {
        public string Query { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string[] IncorrectAnswers { get; set; } = Array.Empty<string>();
    }
}
