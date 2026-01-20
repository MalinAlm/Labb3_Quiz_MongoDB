
using Labb3_Quiz.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Labb3_Quiz.ViewModels
{

    public class QuestionPackViewModel : ViewModelBase
    {
        private readonly QuestionPack _model;
        private readonly Action _saveAction;
        private readonly MainWindowViewModel _mainWindowViewModel;

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
        public QuestionPackViewModel(QuestionPack model, Action saveAction, MainWindowViewModel mainWindowViewModel)
        {
            _model = model;
            _saveAction = saveAction;

            Questions = new ObservableCollection<QuestionViewModel>(
            _model.Questions.Select(q => new QuestionViewModel(q, saveAction, () => mainWindowViewModel.ShowPlayerViewCommand.RaiseCanExecuteChanged())));

            Questions.CollectionChanged += Questions_CollectionChanged;
            _mainWindowViewModel = mainWindowViewModel;
        }


        private void Questions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (QuestionViewModel questionVm in e.NewItems) _model.Questions.Add(questionVm.Model);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (QuestionViewModel questionVm in e.OldItems) _model.Questions.Remove(questionVm.Model);
            }

            _saveAction();
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