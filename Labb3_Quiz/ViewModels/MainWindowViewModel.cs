using Labb3_Quiz.Command;
using Labb3_Quiz.Data.Mongo;
using Labb3_Quiz.Data.Mongo.Documents;
using Labb3_Quiz.Data.Mongo.Repositories;
using Labb3_Quiz.Dialogs;
using Labb3_Quiz.Models;    
using Labb3_Quiz.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace Labb3_Quiz.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly MongoQuizDataService _mongoDataService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly Labb3_Quiz.Data.Mongo.DatabaseSeeder _databaseSeeder;

        public ObservableCollection<QuestionPackViewModel> Packs { get; } = new();
        public PlayerViewModel PlayerViewModel { get; }
        public ConfigurationViewModel ConfigurationViewModel { get; }

        public bool IsConfigurationViewVisible => IsEditMode;
        public bool IsPlayerViewVisible => IsPlayMode;


        private bool _isPlayMode;
        public bool IsPlayMode
        {
            get => _isPlayMode;
            set
            {
                _isPlayMode = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsEditMode));
                RaisePropertyChanged(nameof(IsConfigurationViewVisible));
                RaisePropertyChanged(nameof(IsPlayerViewVisible));
            }
        }

        public bool IsEditMode => !_isPlayMode;

        private QuestionPackViewModel? _activePack;
        public QuestionPackViewModel? ActivePack
		{
			get => _activePack; 
			set {
				_activePack = value;
				RaisePropertyChanged();
				PlayerViewModel?.RaisePropertyChanged(nameof(PlayerViewModel.ActivePack));
                ConfigurationViewModel?.RaisePropertyChanged(nameof(ConfigurationViewModel.ActivePack));

                ShowPlayerViewCommand.RaiseCanExecuteChanged();
                DeletePackCommand.RaiseCanExecuteChanged();
                ImportQuestionsCommand.RaiseCanExecuteChanged(); 
            }
		}

        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                _isFullScreen = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand OpenCreateNewPackDialogCommand { get; }
        public DelegateCommand ShowPlayerViewCommand { get; }
        public DelegateCommand ShowConfigurationViewCommand { get; }
        public DelegateCommand ToggleFullScreenCommand {  get; }
        public DelegateCommand ExitProgramCommand { get; }
        public DelegateCommand SelectPackCommand { get; }
        public DelegateCommand DeletePackCommand { get; }
        public DelegateCommand ImportQuestionsCommand { get; }

        public MainWindowViewModel()
		{

            var settings = new Labb3_Quiz_MongoDB.Data.Mongo.MongoSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "HenrikMalin"
            };


            var context = new Labb3_Quiz_MongoDB.Data.Mongo.MongoDbContext(settings);

            var packRepository = new MongoQuestionPackRepository(context);
            _mongoDataService = new MongoQuizDataService(packRepository);

            _categoryRepository = new MongoCategoryRepository(context);
            _databaseSeeder = new DatabaseSeeder(_categoryRepository, _mongoDataService);



            PlayerViewModel = new PlayerViewModel(this);
			ConfigurationViewModel = new ConfigurationViewModel(this);

            ShowConfigurationViewCommand = new DelegateCommand(_ =>
            {
                IsPlayMode = false;
                PlayerViewModel.StopQuiz();
            });

            ShowPlayerViewCommand = new DelegateCommand(_ => 
            {
                IsPlayMode = true;
                PlayerViewModel.StartQuiz();
            }, _ => ActivePack != null && ActivePack.IsPlayable());

            SelectPackCommand = new DelegateCommand(selectedPack => 
            { 
                if (selectedPack is QuestionPackViewModel pack)
                {
                    ActivePack = pack;
                }
            });

			OpenCreateNewPackDialogCommand = new DelegateCommand(_ => OpenCreateNewPackDialog());
            ToggleFullScreenCommand = new DelegateCommand(_ => IsFullScreen = !IsFullScreen);
            ExitProgramCommand = new DelegateCommand(_ => Application.Current.Shutdown());
            DeletePackCommand = new DelegateCommand(async _ => await DeleteActivePackAsync(), _ => ActivePack != null);
            ImportQuestionsCommand = new DelegateCommand(async _ => await ImportQuestionsAsync(), _ => ActivePack != null);

        }

        public async Task InitializeAsync()
        {
            await _databaseSeeder.EnsureSeedDataAsync();
            await LoadPacksAsync();
        }

        private void OpenCreateNewPackDialog()
		{
			var dialog = new Dialogs.CreateNewPackDialog();
			dialog.DataContext = new CreateNewPackDialogViewModel();

			if (dialog.ShowDialog() == true)
			{
				var dialogViewModel = (CreateNewPackDialogViewModel)dialog.DataContext;

				var newPackModel = new QuestionPack(
                    dialogViewModel.Name, 
                    dialogViewModel.Difficulty,     
                    dialogViewModel.TimeLimitInSeconds);

                var newPack = new QuestionPackViewModel(newPackModel, SaveActivePack, this);
                Packs.Add(newPack);

                ActivePack = newPack;
                ActivePack.SyncToModel();

                _ = SaveActivePackAsync();
            }
		}

        private async Task ImportQuestionsAsync()
        {
            if (ActivePack == null)
            {
                MessageBox.Show("No question pack selected.", "Import",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var api = new Services.TriviaApiService();
                var categories = await api.GetCategoriesAsync();

                if (categories == null || categories.Count == 0)
                {
                    MessageBox.Show(
                        "Couldn't connect to Open Trivia DB.\n\n" +
                        "Check your internet connection and try again.",
                        "Import failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return; 
                }

                var dialog = new Dialogs.ImportQuestionsDialog();
                if (dialog.ShowDialog() == true)
                {
                    if (dialog.ImportedQuestions.Count == 0)
                    {
                        MessageBox.Show("No questions were imported.", "Import",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    foreach (var question in dialog.ImportedQuestions)
                    {
                        ActivePack.Questions.Add(new QuestionViewModel(
                            question,
                            SaveActivePack,
                            () => ShowPlayerViewCommand.RaiseCanExecuteChanged()));
                    }

                    SaveActivePack();
                    ShowPlayerViewCommand.RaiseCanExecuteChanged();

                    MessageBox.Show($"Successfully imported {dialog.ImportedQuestions.Count} questions!",
                        "Import complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Couldn't connect to Open Trivia DB.\n\n" +
                    "Check your internet connection and try again.\n\n" +
                    $"Details: {ex.Message}",
                    "Import failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }


        private async Task LoadPacksAsync()
        {
            var packs = await _mongoDataService.LoadPacksAsync();

            Packs.Clear();

            if (packs.Any())
            {
                foreach (var pack in packs)
                    Packs.Add(new QuestionPackViewModel(pack, SaveActivePack, this));

                ActivePack = Packs.First();
            }
            else
            {
                var newPack = new QuestionPack("Default Pack");
                var vm = new QuestionPackViewModel(newPack, SaveActivePack, this);

                Packs.Add(vm);
                ActivePack = vm;

                await _mongoDataService.UpsertPackAsync(newPack);
            }

            ShowPlayerViewCommand.RaiseCanExecuteChanged();
            DeletePackCommand.RaiseCanExecuteChanged();
            ImportQuestionsCommand.RaiseCanExecuteChanged();

        }


        public void SaveActivePack()
        {
            _ = SaveActivePackAsync();
        }

        public async Task SaveActivePackAsync()
        {
            if (ActivePack == null) return;

            ActivePack.SyncToModel();
            await _mongoDataService.UpsertPackAsync(ActivePack.Model);
        }

        public async Task DeleteActivePackAsync()
        {
            if (ActivePack == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete \"{ActivePack.Name}\"?",
                "Delete Pack",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            PlayerViewModel.StopQuiz();
            IsPlayMode = false;

            var idToDelete = ActivePack.Model.Id;

            Packs.Remove(ActivePack);
            ActivePack = Packs.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(idToDelete))
                await _mongoDataService.DeletePackAsync(idToDelete);

            DeletePackCommand.RaiseCanExecuteChanged();
            ShowPlayerViewCommand.RaiseCanExecuteChanged();
        }

     


    }
}
