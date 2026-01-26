using Labb3_Quiz.ViewModels;
using System.Windows;


namespace Labb3_Quiz
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel? mainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();

            mainWindowViewModel = new MainWindowViewModel();
            DataContext = mainWindowViewModel;

            mainWindowViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(mainWindowViewModel.IsFullScreen))
                {
                    ToggleFullScreen(mainWindowViewModel.IsFullScreen);
                }
            };
        }

        private bool _initialized;

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            if (_initialized) return;
            _initialized = true; 

            try
            {
                await mainWindowViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load packs: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleFullScreen(bool isFullscreen)
        {
            if (isFullscreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            else
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

    }
}