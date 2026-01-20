
using System.Collections.ObjectModel;
using Labb3_Quiz.Command;
using Labb3_Quiz.Models;


namespace Labb3_Quiz.ViewModels
{
   public class CreateNewPackDialogViewModel :ViewModelBase
    {
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

        public CreateNewPackDialogViewModel()
        {
            ConfirmCommand = new DelegateCommand(_ => Confirm());
        }

        private void Confirm()
        {
            RequestClose?.Invoke();
        }
    }
}
