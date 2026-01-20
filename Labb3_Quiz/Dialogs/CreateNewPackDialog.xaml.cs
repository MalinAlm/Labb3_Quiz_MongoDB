
using System.Windows;


namespace Labb3_Quiz.Dialogs
{
    public partial class CreateNewPackDialog : Window
    {

        public CreateNewPackDialog()
        {
            InitializeComponent();

            Loaded += CreateNewPackDialog_Loaded;
        }

            private void CreateNewPackDialog_Loaded(object sender, RoutedEventArgs e)
            {
                if (DataContext is ViewModels.CreateNewPackDialogViewModel viewModel)
                {
                    viewModel.RequestClose += () =>
                    {
                        DialogResult = true;
                        Close();
                    };
                }
            }


    }
}