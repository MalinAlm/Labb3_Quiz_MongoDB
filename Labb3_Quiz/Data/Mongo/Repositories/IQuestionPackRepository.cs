using Labb3_Quiz.Data.Mongo.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb3_Quiz.Data.Mongo.Repositories
{
    internal interface IQuestionPackRepository
    {
        Task<List<QuestionPackDocument>> GetAllAsync();
        Task<QuestionPackDocument> CreateAsync(QuestionPackDocument questionPack);
        Task UpdateAsync(QuestionPackDocument questionPack);
        Task DeleteAsync(string id);
    }
}
