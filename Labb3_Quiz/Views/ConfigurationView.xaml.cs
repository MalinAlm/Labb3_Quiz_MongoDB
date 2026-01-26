using Labb3_Quiz.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Labb3_Quiz.Views
{

    public partial class ConfigurationView : UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not ConfigurationViewModel viewModel) return;

            var command = e.Key switch
            {
                Key.Insert => viewModel.AddQuestionCommand,
                Key.Delete => viewModel.RemoveQuestionCommand,
                _ => null
            };

            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
                e.Handled = true;
            }

        }
    }
}
