// File: ViewModels/MainWindowViewModel.cs

using Labb3_Quiz.Command;
using Labb3_Quiz.Data.Mongo;
using Labb3_Quiz.Data.Mongo.Repositories;
using Labb3_Quiz.Models;
using Labb3_Quiz.Services;
using Labb3_Quiz.Utilities;
using Labb3_Quiz_MongoDB.Data.Mongo;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Labb3_Quiz.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly MongoQuizDataService _mongoDataService;

        private readonly ICategoryRepository _categoryRepository;
        public ICategoryRepository CategoryRepository => _categoryRepository;

        private readonly DatabaseSeeder _databaseSeeder;

        // VG: QuizRuns service (Top5, stats, deletions, etc.)
        private readonly MongoQuizRunService _quizRunService;

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

        private bool _activePackHasRuns;
        public bool ActivePackHasRuns
        {
            get => _activePackHasRuns;
            private set
            {
                if (_activePackHasRuns != value)
                {
                    _activePackHasRuns = value;
                    RaisePropertyChanged();
                }
            }
        }

        private QuestionPackViewModel? _activePack;
        public QuestionPackViewModel? ActivePack
        {
            get => _activePack;
            set
            {
                _activePack = value;
                RaisePropertyChanged();

                // Reset warn-flag when changing pack (warnings should be per pack)
                if (_activePack != null)
                    _activePack.RunInvalidationConfirmed = false;

                PlayerViewModel?.RaisePropertyChanged(nameof(PlayerViewModel.ActivePack));
                ConfigurationViewModel?.RaisePropertyChanged(nameof(ConfigurationViewModel.ActivePack));

                ShowPlayerViewCommand.RaiseCanExecuteChanged();
                DeletePackCommand.RaiseCanExecuteChanged();
                ImportQuestionsCommand.RaiseCanExecuteChanged();

                // Update "has runs" for the newly selected pack
                _ = RefreshActivePackHasRunsAsync();
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
        public DelegateCommand ToggleFullScreenCommand { get; }
        public DelegateCommand ExitProgramCommand { get; }
        public DelegateCommand SelectPackCommand { get; }
        public DelegateCommand DeletePackCommand { get; }
        public DelegateCommand ImportQuestionsCommand { get; }
        public DelegateCommand ManageCategoriesCommand { get; }

        public MainWindowViewModel()
        {
            // NOTE: Deterministic and simple for the assignment (can move to appsettings.json later)
            var settings = new MongoSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "HenrikMalin"
            };

            var context = new MongoDbContext(settings);

            var packRepository = new MongoQuestionPackRepository(context);
            _mongoDataService = new MongoQuizDataService(packRepository);

            _categoryRepository = new MongoCategoryRepository(context);
            _databaseSeeder = new DatabaseSeeder(_categoryRepository, _mongoDataService);

            // VG: runs repo/service
            var runRepository = new MongoQuizRunRepository(context);
            _quizRunService = new MongoQuizRunService(runRepository);

            PlayerViewModel = new PlayerViewModel(this);
            ConfigurationViewModel = new ConfigurationViewModel(this);

            ShowConfigurationViewCommand = new DelegateCommand(_ =>
            {
                IsPlayMode = false;

                // Mid-quiz quit => do NOT save
                PlayerViewModel.CancelRun();
            });

            ShowPlayerViewCommand = new DelegateCommand(_ =>
            {
                // Mandatory name prompt (OK starts, Cancel returns to config)
                if (!TryPromptPlayerName(out var playerName))
                {
                    IsPlayMode = false;
                    return;
                }

                PlayerViewModel.PlayerName = playerName;

                IsPlayMode = true;
                PlayerViewModel.StartQuiz();
            }, _ => ActivePack != null && ActivePack.IsPlayable());

            SelectPackCommand = new DelegateCommand(selectedPack =>
            {
                if (selectedPack is QuestionPackViewModel pack)
                    ActivePack = pack;
            });

            OpenCreateNewPackDialogCommand = new DelegateCommand(_ => OpenCreateNewPackDialog());
            ToggleFullScreenCommand = new DelegateCommand(_ => IsFullScreen = !IsFullScreen);
            ExitProgramCommand = new DelegateCommand(_ => Application.Current.Shutdown());

            DeletePackCommand = new DelegateCommand(async _ => await DeleteActivePackAsync(), _ => ActivePack != null);
            ImportQuestionsCommand = new DelegateCommand(async _ => await ImportQuestionsAsync(), _ => ActivePack != null);
            ManageCategoriesCommand = new DelegateCommand(_ => OpenManageCategoriesDialog());
        }

        public async Task InitializeAsync()
        {
            await _databaseSeeder.EnsureSeedDataAsync();
            await LoadPacksAsync();
        }

        // NOTE:
        // This method is intentionally NON-PUBLIC because PlayerViewModel currently calls it via reflection
        // (BindingFlags.NonPublic). Do not change its name or visibility unless you also update PlayerViewModel.
        private async Task RefreshActivePackHasRunsAsync()
        {
            var packId = ActivePack?.Model?.Id;

            if (string.IsNullOrWhiteSpace(packId))
            {
                ActivePackHasRuns = false;
                return;
            }

            ActivePackHasRuns = await _quizRunService.AnyRunsByPackIdAsync(packId);
        }

        private async void OpenManageCategoriesDialog()
        {
            var dialog = new Dialogs.ManageCategoriesDialog();

            var viewModel = new ManageCategoriesDialogViewModel(_categoryRepository);
            await viewModel.InitializeAsync();

            dialog.DataContext = viewModel;
            dialog.ShowDialog();
        }

        private async void OpenCreateNewPackDialog()
        {
            var dialog = new Dialogs.CreateNewPackDialog();

            var viewModel = new CreateNewPackDialogViewModel(_categoryRepository);
            await viewModel.InitializeAsync();

            dialog.DataContext = viewModel;

            if (dialog.ShowDialog() != true)
                return;

            var newPackModel = new QuestionPack(viewModel.Name, viewModel.Difficulty, viewModel.TimeLimitInSeconds)
            {
                CategoryName = viewModel.SelectedCategory?.Name
            };

            var newPackViewModel = new QuestionPackViewModel(newPackModel, SaveActivePack, this);

            // Baseline fingerprint for new pack (questions-only; excludes name/difficulty/category/time)
            newPackViewModel.LastSavedQuestionsFingerprint =
                PackFingerprint.ComputeQuestionsOnlyHash(newPackViewModel.Model);

            Packs.Add(newPackViewModel);
            ActivePack = newPackViewModel;

            await SaveActivePackAsync();
            await RefreshActivePackHasRunsAsync();
        }

        private async Task ImportQuestionsAsync()
        {
            if (ActivePack == null)
            {
                MessageBox.Show("No question pack selected.", "Import",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Warn once at "edit entry point"
            if (!ConfirmRunInvalidationIfNeeded())
                return;

            try
            {
                var api = new TriviaApiService();
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
                if (dialog.ShowDialog() != true)
                    return;

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
                {
                    var packViewModel = new QuestionPackViewModel(pack, SaveActivePack, this);

                    packViewModel.LastSavedQuestionsFingerprint =
                        PackFingerprint.ComputeQuestionsOnlyHash(packViewModel.Model);

                    Packs.Add(packViewModel);
                }

                ActivePack = Packs.First();
            }
            else
            {
                var newPack = new QuestionPack("Default Pack");
                var packViewModel = new QuestionPackViewModel(newPack, SaveActivePack, this);

                packViewModel.LastSavedQuestionsFingerprint =
                    PackFingerprint.ComputeQuestionsOnlyHash(packViewModel.Model);

                Packs.Add(packViewModel);
                ActivePack = packViewModel;

                await _mongoDataService.UpsertPackAsync(newPack);
            }

            ShowPlayerViewCommand.RaiseCanExecuteChanged();
            DeletePackCommand.RaiseCanExecuteChanged();
            ImportQuestionsCommand.RaiseCanExecuteChanged();

            await RefreshActivePackHasRunsAsync();
        }

        public void SaveActivePack() => _ = SaveActivePackAsync();

        public async Task SaveActivePackAsync()
        {
            if (ActivePack == null)
                return;

            ActivePack.SyncToModel();

            // Fingerprint = only question content (NOT name/difficulty/category/time)
            var currentFingerprint = PackFingerprint.ComputeQuestionsOnlyHash(ActivePack.Model);
            var previousFingerprint = ActivePack.LastSavedQuestionsFingerprint ?? string.Empty;

            var questionsChanged = currentFingerprint != previousFingerprint;

            // If questions changed AND user confirmed invalidation AND there were runs: delete runs once before save
            if (questionsChanged && ActivePack.RunInvalidationConfirmed && ActivePackHasRuns)
            {
                var packId = ActivePack.Model.Id;

                if (!string.IsNullOrWhiteSpace(packId))
                {
                    await _quizRunService.DeleteRunsByPackIdAsync(packId);
                    ActivePackHasRuns = false;
                }
            }

            await _mongoDataService.UpsertPackAsync(ActivePack.Model);

            // After save: update baseline and reset confirmation
            ActivePack.LastSavedQuestionsFingerprint = currentFingerprint;
            ActivePack.RunInvalidationConfirmed = false;

            await RefreshActivePackHasRunsAsync();
        }

        public async Task DeleteActivePackAsync()
        {
            if (ActivePack == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete \"{ActivePack.Name}\"?",
                "Delete Pack",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            PlayerViewModel.CancelRun();
            IsPlayMode = false;

            var idToDelete = ActivePack.Model.Id;

            Packs.Remove(ActivePack);
            ActivePack = Packs.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(idToDelete))
            {
                await _mongoDataService.DeletePackAsync(idToDelete);
                await _quizRunService.DeleteRunsByPackIdAsync(idToDelete);
            }

            DeletePackCommand.RaiseCanExecuteChanged();
            ShowPlayerViewCommand.RaiseCanExecuteChanged();

            await RefreshActivePackHasRunsAsync();
        }

        public bool ConfirmRunInvalidationIfNeeded()
        {
            if (ActivePack == null)
                return false;

            if (!ActivePackHasRuns)
                return true;

            if (ActivePack.RunInvalidationConfirmed)
                return true;

            var result = MessageBox.Show(
                "This pack has saved play sessions.\n\n" +
                "If you continue, all saved sessions will be erased.\n\n" +
                "Do you want to continue?",
                "Warning",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return false;

            ActivePack.RunInvalidationConfirmed = true;
            return true;
        }

        private bool TryPromptPlayerName(out string playerName)
        {
            playerName = string.Empty;

            var dialog = new Dialogs.PlayerNameDialog();
            var viewModel = new PlayerNameDialogViewModel(PlayerViewModel.PlayerName);

            dialog.DataContext = viewModel;

            var ok = dialog.ShowDialog() == true;
            if (!ok)
                return false;

            var trimmedName = (viewModel.PlayerName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                MessageBox.Show("Please enter a name to start playing.", "Name required",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            playerName = trimmedName;
            return true;
        }
    }
}
