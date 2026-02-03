// File: Dialogs/PlayerNameDialog.xaml.cs

using System.Windows;
using Labb3_Quiz.ViewModels;

namespace Labb3_Quiz.Dialogs
{
    /// <summary>
    /// Interaction logic for PlayerNameDialog.xaml
    /// </summary>
    public partial class PlayerNameDialog : Window
    {
        public PlayerNameDialog()
        {
            InitializeComponent();

            // Wire up ViewModel → Window closing callbacks
            Loaded += (_, _) =>
            {
                if (DataContext is not PlayerNameDialogViewModel viewModel)
                    return;

                viewModel.RequestCloseOk = () =>
                {
                    DialogResult = true;
                    Close();
                };

                viewModel.RequestCloseCancel = () =>
                {
                    DialogResult = false;
                    Close();
                };
            };
        }
    }
}
