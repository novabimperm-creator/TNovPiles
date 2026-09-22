using System.Windows;
using System.Windows.Input;
using TNovCommon;

namespace TNovPiles
{
    /// <summary>
    /// Логика взаимодействия для FoundNumWPF.xaml
    /// </summary>
    public partial class FoundNumWPF : Window
    {
        public FoundNumWPF(FoundNumViewModel viewModel)
        {
            InitializeComponent();
            textBox1.Focus();
            DataContext = viewModel;

            this.SizeToContent = SizeToContent.Height;
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
