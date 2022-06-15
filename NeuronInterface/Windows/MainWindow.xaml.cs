using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex("[^0-9.-]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    private void PositiveNumber(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex(@"^\d*\.?\d*$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void Initialize()
    {
        if (!decimal.TryParse(LearningRate.Text, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"),
                out var learningRate))
            throw new Exception();

        InterfaceApp.LearningRate = learningRate;

        switch (InterfaceApp.Mode)
        {
            case InterfaceApp.MODE.Error:
            {
                if (!decimal.TryParse(StopConditionTextBox.Text, NumberStyles.AllowDecimalPoint,
                        CultureInfo.GetCultureInfo("en-US"), out var maxError))
                    throw new Exception();

                    InterfaceApp.MaxError = maxError;
                    break;
                }
            case InterfaceApp.MODE.Iterations:
            {
                if (!int.TryParse(StopConditionTextBox.Text, out var iterationStep))
                    throw new Exception();

                    InterfaceApp.IterationStep = iterationStep;
                    break;
                }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void EditData_Click(object sender, RoutedEventArgs e)
    {
        var dataWindow = new DataWindow();
        dataWindow.Owner = this;
        dataWindow.ShowDialog();
    }

    private void NeuronType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = (ComboBoxItem)e.AddedItems[0];

        if (item.Content.Equals("Perceptron"))
            InterfaceApp.Neuron = InterfaceApp.NEURON.Perceptron;
        if (item.Content.Equals("Adaline"))
            InterfaceApp.Neuron = InterfaceApp.NEURON.Adaline;
    }

    private void StopCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = (ComboBoxItem)e.AddedItems[0];

        if (item.Content.Equals("Próg błędu"))
            InterfaceApp.Mode = InterfaceApp.MODE.Error;

        if (item.Content.Equals("Ilość iteracji"))
            InterfaceApp.Mode = InterfaceApp.MODE.Iterations;
    }

    private void Solve_Click(object sender, RoutedEventArgs e)
    {
        Initialize();

        InterfaceApp.Solve();
    }

    private void LoadData_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Wczytaj dane"
        };
        openFileDialog.ShowDialog();
        InterfaceApp.LoadDataFromFile(openFileDialog.FileName);
    }
}