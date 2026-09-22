using System.Windows;
using System.Windows.Input;
using TNovCommon;

namespace TNovPiles
{
    /// <summary>
    /// Логика взаимодействия для FoundWPF.xaml
    /// </summary>
    public partial class FoundWPF : Window
    {
        public FoundWPF(FoundViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            this.SizeToContent = SizeToContent.Height;
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void escButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpLinks.ShowHelp("Сваи");
        }
    }
}
