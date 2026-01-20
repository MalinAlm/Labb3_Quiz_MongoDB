using Labb3_Quiz.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
