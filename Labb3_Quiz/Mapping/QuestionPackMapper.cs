using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Dialogs;
using Labb3_Quiz.Models;
using Labb3_Quiz.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Labb3_Quiz.Mapping
{
    public static class QuestionPackMapper
    {
        public static QuestionPack QuestionPackDocumentToModelMap(this QuestionPackDocument doc)
        {
            var model = new QuestionPack(doc.Name, doc.Difficulty, doc.TimeLimitInSeconds);

            if (doc.Questions != null)
            {
                model.Questions = doc.Questions
                    .Select(qd => qd.QuestionDocumentToModelMap())
                    .ToList();
            }

            return model;
        }

        public static QuestionPackDocument ModelToQuestionDocument(this QuestionPack model)
        {
            QuestionPackDocument _questionPackDocument = new QuestionPackDocument();
            _questionPackDocument.Name = model.Name;
            _questionPackDocument.Difficulty = model.Difficulty;
            _questionPackDocument.TimeLimitInSeconds = model.TimeLimitInSeconds;

            if (model.Questions != null)
            {
                // Create a List<QuestionDocument> and map each Question -> QuestionDocument
                var questionDocList = new List<QuestionDocument>();
                foreach (var q in model.Questions)
                {
                    // use the model -> document mapper
                    questionDocList.Add(q.ModelToQuestionDocument());
                }

                _questionPackDocument.Questions = questionDocList;
            }

            return _questionPackDocument;
        }
    }
}
