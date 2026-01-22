using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Labb3_Quiz.Mapping
{
    public static class QuestionMapper
    {
        public static Question QuestionDocumentToModelMap(this QuestionDocument doc)
        {

            return new Question(doc.Query, doc.CorrectAnswer, doc.IncorrectAnswers[0], doc.IncorrectAnswers[1], doc.IncorrectAnswers[2]);
        }

        public static QuestionDocument ModelToQuestionDocument(this Question model)
        {

            QuestionDocument _questionDocument = new QuestionDocument();

            _questionDocument.Query = model.Query;
            _questionDocument.CorrectAnswer = model.CorrectAnswer;
            _questionDocument.IncorrectAnswers = model.IncorrectAnswers;
            return _questionDocument;
        }



    }
}
