using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Labb3_Quiz.Models;
using Labb3_Quiz.Utilities;
using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Data.Mongo.Repositories;


namespace Labb3_Quiz.ViewModels
{

    public class PackOptionsDialogViewModel : ViewModelBase
    {

        private readonly ICategoryRepository _categoryRepository;
        private readonly string? _currentCategoryName;

        private string _name;
        public string Name
        {
            get => _name;
            set 
            { 
                _name = value; 
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<CategoryDocument> AvailableCategories { get; } = new();
        

        private CategoryDocument? _selectedCategory;
        public CategoryDocument? SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<Difficulty> Difficulties { get; } =
           new(Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>());

        private Difficulty _difficulty;
        public Difficulty Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                RaisePropertyChanged();
            }
        }

        private int _timeLimitInSeconds;
        public int TimeLimitInSeconds
        {
            get => _timeLimitInSeconds;
            set
            {
                _timeLimitInSeconds = value;
                RaisePropertyChanged();
            }
        }

        public PackOptionsDialogViewModel(QuestionPack pack, ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
            _name = pack.Name;
            _difficulty = pack.Difficulty;
            _timeLimitInSeconds = pack.TimeLimitInSeconds;

            _currentCategoryName = pack.CategoryName;

        }

        public async Task InitializeAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            AvailableCategories.Clear();
            foreach (var c in categories)
                AvailableCategories.Add(c);

            SelectedCategory = AvailableCategories
                .FirstOrDefault(c => c.Name == _currentCategoryName)
                ?? AvailableCategories.FirstOrDefault();
        }


        public void ApplyChanges(QuestionPack pack)
        {
            pack.Name = _name;
            pack.Difficulty = _difficulty;
            pack.TimeLimitInSeconds = _timeLimitInSeconds;
            pack.CategoryName = SelectedCategory?.Name;
        }
    }
}
