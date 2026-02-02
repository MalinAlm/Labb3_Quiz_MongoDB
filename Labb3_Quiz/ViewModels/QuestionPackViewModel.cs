// File: ViewModels/QuestionPackViewModel.cs

using Labb3_Quiz.Models;
using Labb3_Quiz.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Labb3_Quiz.ViewModels
{
    public class QuestionPackViewModel : ViewModelBase
    {
        private readonly QuestionPack _model;
        private readonly Action _saveAction;
        private readonly MainWindowViewModel _mainWindowViewModel;

        // --- Step 0.3 additions (runs/fingerprint state) ---

        // Fingerprint of question-content that was last saved/loaded (questions only).
        // OBS: excludes Name/Difficulty/Category/TimeLimit (per our plan).
        private string _lastSavedQuestionsFingerprint = string.Empty;
        public string LastSavedQuestionsFingerprint
        {
            get => _lastSavedQuestionsFingerprint;
            set
            {
                _lastSavedQuestionsFingerprint = value;
                RaisePropertyChanged();
            }
        }

        // True after user accepted “this will erase sessions” warning once.
        // (we will use this in Step 0.4/0.5)
        private bool _runInvalidationConfirmed;
        public bool RunInvalidationConfirmed
        {
            get => _runInvalidationConfirmed;
            set
            {
                _runInvalidationConfirmed = value;
                RaisePropertyChanged();
            }
        }

        // --- Existing VM props ---

        public string Name
        {
            get => _model.Name;
            set
            {
                _model.Name = value;
                RaisePropertyChanged();
                _saveAction();
            }
        }

        public string CategoryName
        {
            get => _model.CategoryName ?? string.Empty;
            set
            {
                _model.CategoryName = value;
                RaisePropertyChanged();
                _saveAction();
            }
        }

        public Difficulty Difficulty
        {
            get => _model.Difficulty;
            set
            {
                _model.Difficulty = value;
                RaisePropertyChanged();
                _saveAction();
            }
        }

        public int TimeLimitInSeconds
        {
            get => _model.TimeLimitInSeconds;
            set
            {
                _model.TimeLimitInSeconds = value;
                RaisePropertyChanged();
                _saveAction();
            }
        }

        public ObservableCollection<QuestionViewModel> Questions { get; }

        public QuestionPackViewModel(
            QuestionPack model,
            Action saveAction,
            MainWindowViewModel mainWindowViewModel)
        {
            _model = model;
            _saveAction = saveAction;
            _mainWindowViewModel = mainWindowViewModel;

            Questions = new ObservableCollection<QuestionViewModel>(
                _model.Questions.Select(q =>
                    new QuestionViewModel(
                        q,
                        saveAction,
                        () => mainWindowViewModel.ShowPlayerViewCommand.RaiseCanExecuteChanged()
                    )
                )
            );

            Questions.CollectionChanged += Questions_CollectionChanged;
        }

        private void Questions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (QuestionViewModel questionViewModel in e.NewItems)
                    _model.Questions.Add(questionViewModel.Model);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (QuestionViewModel questionViewModel in e.OldItems)
                    _model.Questions.Remove(questionViewModel.Model);
            }

            _saveAction();
            _mainWindowViewModel.ShowPlayerViewCommand.RaiseCanExecuteChanged();
        }

        public bool IsPlayable()
        {
            return Questions.Any() && Questions.All(q =>
                !string.IsNullOrWhiteSpace(q.Query) &&
                !string.IsNullOrWhiteSpace(q.CorrectAnswer) &&
                !string.IsNullOrWhiteSpace(q.IncorrectAnswer1) &&
                !string.IsNullOrWhiteSpace(q.IncorrectAnswer2) &&
                !string.IsNullOrWhiteSpace(q.IncorrectAnswer3));
        }

        public void SyncToModel()
        {
            _model.Questions = Questions.Select(qvm => qvm.Model).ToList();
        }

        public QuestionPack Model => _model;
    }
}
