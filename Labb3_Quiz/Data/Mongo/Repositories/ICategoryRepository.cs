using Labb3_Quiz.Data.Mongo.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb3_Quiz.Data.Mongo.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<CategoryDocument>> GetAllAsync();
        Task<CategoryDocument> CreateAsync(CategoryDocument category);
        Task UpdateAsync(CategoryDocument category);
        Task DeleteAsync(string id);
    }
}
