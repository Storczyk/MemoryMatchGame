using System.Windows;


namespace MemoryMatchGame
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            PlayArea arena = new PlayArea();
            arena.StartArea(playArea,textBlock, startButton);
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
