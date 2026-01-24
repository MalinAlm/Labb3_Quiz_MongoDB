
using Labb3_Quiz.Command;
using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Data.Mongo.Repositories;
using Labb3_Quiz.Models;
using Labb3_Quiz.Utilities;
using System.Collections.ObjectModel;


namespace Labb3_Quiz.ViewModels
{
   public class CreateNewPackDialogViewModel :ViewModelBase
    {

        private readonly ICategoryRepository _categoryRepository;
        public ObservableCollection<CategoryDocument> AvailableCategories { get; } = new();


        private CategoryDocument? _selectedCategory;
        public CategoryDocument? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; RaisePropertyChanged(); }
        }

        private string _name = "New Pack";
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<Difficulty> Difficulties { get; } = new(Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>());
        public Difficulty Difficulty { get; set; } = Difficulty.Medium;

        private int _timeLimitInSeconds = 30;
        public int TimeLimitInSeconds 
        { 
            get => _timeLimitInSeconds;
            set
            {
                if (_timeLimitInSeconds != value)
                {
                    _timeLimitInSeconds = value;
                    RaisePropertyChanged();
                }
            }
        }

        public DelegateCommand ConfirmCommand  { get; }

        public event Action? RequestClose;

        public CreateNewPackDialogViewModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            ConfirmCommand = new DelegateCommand(_ => Confirm());
        }

        private void Confirm()
        {
            RequestClose?.Invoke();
        }

        public async Task InitializeAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            AvailableCategories.Clear();
            foreach (var c in categories)
                AvailableCategories.Add(c);

            SelectedCategory = AvailableCategories.FirstOrDefault();
        }
    }
}
