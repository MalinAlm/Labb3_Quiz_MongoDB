
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Labb3_Quiz.Services
{
    public class TriviaApiService
    {
        private readonly HttpClient _httpClient;

        public TriviaApiService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(6)
            };
        }

        public class TriviaCategoryResponse
        {
            [JsonPropertyName("trivia_categories")]
            public List<TriviaCategory> TriviaCategories { get; set; } = new();
        }

        public class TriviaCategory
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; } = "";
        }

        public class TriviaQuestionResponse
        {
            [JsonPropertyName("response_code")]
            public int ResponseCode { get; set; }

            [JsonPropertyName("results")]
            public List<TriviaQuestion> Results { get; set; } = new();
        }

        public class TriviaQuestion
        {
            [JsonPropertyName("question")]
            public string Question { get; set; } = "";

            [JsonPropertyName("correct_answer")]
            public string CorrectAnswer { get; set; } = "";

            [JsonPropertyName("incorrect_answers")]
            public List<string> IncorrectAnswers { get; set; } = new();
        }

        public async Task<List<TriviaCategory>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TriviaCategoryResponse>("https://opentdb.com/api_category.php");

                return response?.TriviaCategories ?? new();
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error getting category: {ex}");
                return new List<TriviaCategory>();
            }
        }

        public async Task<List<TriviaQuestion>> GetQuestionsAsync(int categoryId, string difficulty, int amount)
        {
            try
            {
                var url = $"https://opentdb.com/api.php?amount={amount}&category={categoryId}&difficulty={difficulty}&type=multiple&encode=url3986";

                var response = await _httpClient.GetFromJsonAsync<TriviaQuestionResponse>(url);

                if (response == null || response.ResponseCode != 0) return new();

                foreach (var q in response.Results)
                {
                    q.Question = Uri.UnescapeDataString(q.Question);
                    q.CorrectAnswer = Uri.UnescapeDataString(q.CorrectAnswer);
                    q.IncorrectAnswers = q.IncorrectAnswers
                        .Select(a => Uri.UnescapeDataString(a)).ToList();
                }

                return response.Results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Quetions: {ex}");
                return new List<TriviaQuestion>();
            }
        }
    }
}
