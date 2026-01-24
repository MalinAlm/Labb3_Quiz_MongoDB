using System.Collections.ObjectModel;
using Labb3_Quiz.Command;
using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Data.Mongo.Repositories;

namespace Labb3_Quiz.ViewModels
{
    public class ManageCategoriesDialogViewModel : ViewModelBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public ObservableCollection<CategoryDocument> Categories { get; } = new();

        private CategoryDocument? _selectedCategory;
        public CategoryDocument? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                RaisePropertyChanged();
                CategoryNameInput = _selectedCategory?.Name ?? string.Empty;
                RaiseCommandStates();
            }
        }

        private string _categoryNameInput = string.Empty;
        public string CategoryNameInput
        {
            get => _categoryNameInput;
            set
            {
                _categoryNameInput = value;
                RaisePropertyChanged();
                RaiseCommandStates();
            }
        }

        public DelegateCommand AddCategoryCommand { get; }
        public DelegateCommand RenameCategoryCommand { get; }
        public DelegateCommand DeleteCategoryCommand { get; }

        public ManageCategoriesDialogViewModel(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            AddCategoryCommand = new DelegateCommand(async _ => await AddAsync(), _ => CanAdd());
            RenameCategoryCommand = new DelegateCommand(async _ => await RenameAsync(), _ => CanRename());
            DeleteCategoryCommand = new DelegateCommand(async _ => await DeleteAsync(), _ => CanDelete());
        }

        public async Task InitializeAsync() => await ReloadAsync();

        private async Task ReloadAsync()
        {
            var docs = await _categoryRepository.GetAllAsync();
            Categories.Clear();
            foreach (var c in docs)
                Categories.Add(c);

            if (SelectedCategory != null)
                SelectedCategory = Categories.FirstOrDefault(x => x.Id == SelectedCategory.Id);
        }

        private bool CanAdd() =>
            !string.IsNullOrWhiteSpace(CategoryNameInput);

        private bool CanRename() =>
            SelectedCategory != null && !string.IsNullOrWhiteSpace(CategoryNameInput);

        private bool CanDelete() =>
            SelectedCategory != null;

        private async Task AddAsync()
        {
            await _categoryRepository.CreateAsync(new CategoryDocument { Name = CategoryNameInput.Trim() });
            CategoryNameInput = string.Empty;
            await ReloadAsync();
        }

        private async Task RenameAsync()
        {
            if (SelectedCategory == null) return;

            SelectedCategory.Name = CategoryNameInput.Trim();
            await _categoryRepository.UpdateAsync(SelectedCategory);
            await ReloadAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedCategory == null) return;

            await _categoryRepository.DeleteAsync(SelectedCategory.Id);
            SelectedCategory = null;
            CategoryNameInput = string.Empty;
            await ReloadAsync();
        }

        private void RaiseCommandStates()
        {
            AddCategoryCommand.RaiseCanExecuteChanged();
            RenameCategoryCommand.RaiseCanExecuteChanged();
            DeleteCategoryCommand.RaiseCanExecuteChanged();
        }
    }
}
