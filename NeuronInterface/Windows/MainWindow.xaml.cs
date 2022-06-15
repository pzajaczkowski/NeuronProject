using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace NeuronInterface.Windows;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }


    private void EditData_Click(object sender, RoutedEventArgs e)
    {
        var dataWindow = new DataWindow();
        dataWindow.Owner = this;
        dataWindow.ShowDialog();
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex("[^0-9.-]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}