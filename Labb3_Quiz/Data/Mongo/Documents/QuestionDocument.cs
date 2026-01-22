using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb3_Quiz.Data.Mongo.Documents
{
    public class QuestionDocument
    {
        public string Query { get; set; }
        public string CorrectAnswer { get; set; }
        public string[] IncorrectAnswers { get; set; } = Array.Empty<string>();

    }
}
