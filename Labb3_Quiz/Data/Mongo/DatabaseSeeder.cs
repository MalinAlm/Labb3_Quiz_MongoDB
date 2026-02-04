using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Data.Mongo.Repositories;
using Labb3_Quiz.Models;
using Labb3_Quiz.Services;
using Labb3_Quiz.Utilities;

namespace Labb3_Quiz.Data.Mongo
{
    public class DatabaseSeeder
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly MongoQuizDataService _quizDataService;

        public DatabaseSeeder(ICategoryRepository categoryRepository, MongoQuizDataService quizDataService)
        {
            _categoryRepository = categoryRepository;
            _quizDataService = quizDataService;
        }

        public async Task EnsureSeedDataAsync()
        {
            await EnsureCategoriesAsync();
            await EnsureDemoPackAsync();
        }

        private async Task EnsureCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            if (categories.Count > 0) return;

            var defaultNames = new[] { "IT", "History", "Science", "Sports" };

            foreach (var name in defaultNames)
                await _categoryRepository.CreateAsync(new CategoryDocument { Name = name });
        }

        private async Task EnsureDemoPackAsync()
        {
            var packs = await _quizDataService.LoadPacksAsync();
            if (packs.Count > 0) return;

            var demoPack = new QuestionPack("Demo Pack", Difficulty.Medium, 30)
            {
                CategoryName = "IT",
                Questions = new List<Question>
                {
                    // new Question("Is Fredrik 1337 ;D ? ", "yes!", "no", "no.jpg", "nopedyno"), 
                    new Question("Vad står CPU för?",
                        "Central Processing Unit",
                        "Computer Personal Unit",
                        "Core Power Utility",
                        "Central Process Utility"),
                    new Question("Vilken av dessa är en NoSQL-databas?",
                        "MongoDB",
                        "PostgreSQL",
                        "MySQL",
                        "SQLite"),
                    new Question("Vilket nyckelord skapar ett objekt i C#?",
                        "new",
                        "create",
                        "make",
                        "init"),
                    new Question("Vilken typ används oftast för listor i C#?",
                        "List<T>",
                        "Array<T>",
                        "Map<T>",
                        "Set<T>"),
                    new Question("Vad heter MongoDBs standard-id-fält?",
                        "_id",
                        "id",
                        "objectId",
                        "mongoId")
                }
            };

            await _quizDataService.UpsertPackAsync(demoPack);
        }
    }
}
