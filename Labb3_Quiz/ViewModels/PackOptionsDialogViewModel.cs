using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Labb3_Quiz.Models;

namespace Labb3_Quiz.ViewModels
{
    public class PackOptionsDialogViewModel : ViewModelBase
    {
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

        public ObservableCollection<Difficulty> Difficulties { get; } =
            new(Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>());

        public PackOptionsDialogViewModel(QuestionPack pack)
        {
            _name = pack.Name;
            _difficulty = pack.Difficulty;
            _timeLimitInSeconds = pack.TimeLimitInSeconds;
        }

        public void ApplyChanges(QuestionPack pack)
        {
            pack.Name = _name;
            pack.Difficulty = _difficulty;
            pack.TimeLimitInSeconds = _timeLimitInSeconds;
        }
    }
}
