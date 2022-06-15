using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace NeuronInterface.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly InterfaceApp app;
        public MainWindow()
        {
            InitializeComponent();
            app = new InterfaceApp();
        }


        private void EditData_Click(object sender, RoutedEventArgs e)
        {
            var dataWindow = new DataWindow(app);
            dataWindow.Owner = this;
            dataWindow.ShowDialog();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
