using Labb3_Quiz.Models;

namespace Labb3_Quiz.ViewModels
{
    public class QuestionViewModel : ViewModelBase
    {
        private readonly Question _model;
        private readonly Action _saveAction;
        private readonly Action _updatePlayCommand;

        public QuestionViewModel(Question model, Action saveAction, Action updatePlayCommand)
        {
            _model = model;
            _saveAction = saveAction;
            _updatePlayCommand = updatePlayCommand;

            ValidateIncorrectAnswers();
        }

        public Question Model => _model;

        public string Query
        {
            get => _model.Query;
            set
            {
                _model.Query = value;
                RaisePropertyChanged();
                _saveAction();
                _updatePlayCommand();
            }
        }

        public string CorrectAnswer
        {
            get => _model.CorrectAnswer;
            set
            {
                _model.CorrectAnswer = value;
                RaisePropertyChanged();
                _saveAction();
                _updatePlayCommand();
            }
        }

        public string IncorrectAnswer1
        {
            get => _model.IncorrectAnswers[0];
            set
            {
                _model.IncorrectAnswers[0] = value;
                RaisePropertyChanged();
                _saveAction();
                _updatePlayCommand();
            }
        }
        public string IncorrectAnswer2
        {
            get => _model.IncorrectAnswers[1];
            set
            {
                _model.IncorrectAnswers[1] = value;
                RaisePropertyChanged();
                _saveAction();
                _updatePlayCommand();
            }
        }
        public string IncorrectAnswer3
        {
            get => _model.IncorrectAnswers[2];
            set
            {
                _model.IncorrectAnswers[2] = value;
                RaisePropertyChanged();
                _saveAction();
                _updatePlayCommand();
            }
        }

        private void ValidateIncorrectAnswers()
        {
            if (_model.IncorrectAnswers == null || _model.IncorrectAnswers.Length != 3)
            {
                _model.IncorrectAnswers = new string[3] { "", "", "" };
            }
        }
    }
}
