using Labb3_Quiz.Models;
using System.Windows;
using Labb3_Quiz.Services;


namespace Labb3_Quiz.Dialogs
{
    public partial class ImportQuestionsDialog : Window
    {
        private readonly TriviaApiService _apiService;

        public List<Question> ImportedQuestions { get; private set; } = new();

        public ImportQuestionsDialog()
        {
            InitializeComponent();
            _apiService = new TriviaApiService();

            AmountSlider.ValueChanged += (sender, args) =>
            {
                AmountTextBlock.Text = ((int)AmountSlider.Value).ToString();
            };

            DifficultyCombobox.ItemsSource = new[] { "Easy", "Medium", "Hard" };
            DifficultyCombobox.SelectedIndex = 0;

            Loaded += ImportQuestionsDialog_Loaded;
        }

        private async void ImportQuestionsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            CategoryComboBox.Items.Clear();

            var categories = await _apiService.GetCategoriesAsync();

            if (categories.Count == 0)
            {
                MessageBox.Show("Failed to load categories from OpenTdB", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.DisplayMemberPath = "Name";
            CategoryComboBox.SelectedValuePath = "Id";
            CategoryComboBox.SelectedIndex = 0;
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show($"Please select a category and difficulty.", 
                    "Missing data", 
                    MessageBoxButton.OK,    
                    MessageBoxImage.Warning);

                return;
            }

            if (DifficultyCombobox.SelectedItem is not string difficulty)
            {
                MessageBox.Show($"Please select a category and difficulty.", 
                    "Missing data", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);

                return;
            }

            int categoryId = (int)CategoryComboBox.SelectedValue;
            int amount = (int)AmountSlider.Value;

            var questions = await _apiService.GetQuestionsAsync(categoryId, difficulty.ToLower(), amount);

            if (questions.Count == 0)
            {
                MessageBox.Show("The API returned no valid questions", 
                    "Import failed", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);

                return;
            }

            //Convert API to internal Question model
            ImportedQuestions = questions.Select(q => new Question(
                q.Question,
                q.CorrectAnswer,
                q.IncorrectAnswers.ElementAtOrDefault(0) ?? "",
                q.IncorrectAnswers.ElementAtOrDefault(1) ?? "",
                q.IncorrectAnswers.ElementAtOrDefault(2) ?? ""
                )).ToList();

            DialogResult = true;
            Close();
        }
            
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
