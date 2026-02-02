using Labb3_Quiz.Command;
using Labb3_Quiz.Models;

namespace Labb3_Quiz.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {

        public bool HasActivePack => ActivePack != null;

        private readonly MainWindowViewModel _mainWindowViewModel;
        public QuestionPackViewModel? ActivePack { get => _mainWindowViewModel?.ActivePack; }

        public bool HasActiveQuestion => ActiveQuestion != null;

        private QuestionViewModel? _activeQuestion;
        public QuestionViewModel? ActiveQuestion
        {
            get => _activeQuestion;
            set
            {
                _activeQuestion = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ActiveQuestion));
                System.Diagnostics.Debug.WriteLine($"ActiveQuestion set: {_activeQuestion?.Query ?? "null"}");


                RemoveQuestionCommand?.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand AddQuestionCommand { get; }
        public DelegateCommand RemoveQuestionCommand { get; }
        public DelegateCommand OpenPackOptionsDialogCommand { get; }

        public ConfigurationViewModel(MainWindowViewModel mainWindowViewModel) 
        {
            this._mainWindowViewModel = mainWindowViewModel;

            _mainWindowViewModel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(_mainWindowViewModel.ActivePack))
                {
                    RaisePropertyChanged(nameof(ActivePack));
                    RaisePropertyChanged(nameof(HasActivePack));

                    AddQuestionCommand.RaiseCanExecuteChanged();
                    OpenPackOptionsDialogCommand.RaiseCanExecuteChanged();
                    RemoveQuestionCommand.RaiseCanExecuteChanged();
                }
            };

            AddQuestionCommand = new DelegateCommand(_ => AddQuestion(), _ => HasActivePack);
            OpenPackOptionsDialogCommand = new DelegateCommand(_ => OpenPackoptionsDialog(), _ => HasActivePack);
            RemoveQuestionCommand = new DelegateCommand(_ => RemoveQuestion(), _ => CanRemoveQuestion());
        }

        private void AddQuestion()
        {
            if (ActivePack == null) return;

            // warn once before destructive edits
            if (!CanProceedWithQuestionContentChange())
                return;

            var newQuestionModel = new Question("New Question", string.Empty, string.Empty, string.Empty, string.Empty);

            var newQuestionViewModel = new QuestionViewModel(newQuestionModel, _mainWindowViewModel.SaveActivePack,
                () => _mainWindowViewModel.ShowPlayerViewCommand.RaiseCanExecuteChanged());
                
            ActivePack.Questions.Add(newQuestionViewModel);
            ActiveQuestion = newQuestionViewModel;

            _mainWindowViewModel.ShowPlayerViewCommand.RaiseCanExecuteChanged();
        }
        
        private void RemoveQuestion()
        {
            if (ActivePack == null || ActiveQuestion == null) return;

            // warn once before destructive edits
            if (!CanProceedWithQuestionContentChange())
                return;

            ActivePack.Questions.Remove(ActiveQuestion);
            ActiveQuestion = null;

            _mainWindowViewModel.ShowPlayerViewCommand.RaiseCanExecuteChanged();
        }

        private bool CanProceedWithQuestionContentChange()
        {
            return _mainWindowViewModel.ConfirmRunInvalidationIfNeeded();
        }

        private bool CanRemoveQuestion() => ActiveQuestion != null;
    
        private async void OpenPackoptionsDialog()
        {

            if (ActivePack == null) return;

            var dialog = new Dialogs.PackOptionsDialog();

            var viewModel = new PackOptionsDialogViewModel(ActivePack.Model, _mainWindowViewModel.CategoryRepository);
            await viewModel.InitializeAsync();

            dialog.DataContext = viewModel;
            dialog.ShowDialog();

            viewModel.ApplyChanges(ActivePack.Model);


            ActivePack.Name = viewModel.Name;
            ActivePack.Difficulty = viewModel.Difficulty;
            ActivePack.TimeLimitInSeconds = viewModel.TimeLimitInSeconds;
            ActivePack.CategoryName = viewModel.SelectedCategory?.Name ?? string.Empty;


            _mainWindowViewModel.SaveActivePack();

        }
    }
}
