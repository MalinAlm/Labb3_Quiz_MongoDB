using Labb3_Quiz.Command;

namespace Labb3_Quiz.ViewModels
{
    public class PlayerNameDialogViewModel : ViewModelBase
    {
        private string _playerName;

        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        /// <summary>
        /// Set by the dialog (code-behind) to close with DialogResult = true
        /// </summary>
        public Action? RequestCloseOk { get; set; }

        /// <summary>
        /// Set by the dialog (code-behind) to close with DialogResult = false
        /// </summary>
        public Action? RequestCloseCancel { get; set; }

        public PlayerNameDialogViewModel(string? initialName = null)
        {
            _playerName = initialName?.Trim() ?? string.Empty;

            OkCommand = new DelegateCommand(_ => OnOk());
            CancelCommand = new DelegateCommand(_ => OnCancel());
        }

        private void OnOk()
        {
            RequestCloseOk?.Invoke();
        }

        private void OnCancel()
        {
            RequestCloseCancel?.Invoke();
        }
    }
}
