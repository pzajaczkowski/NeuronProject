using Microsoft.Win32;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
                    if (!ulong.TryParse(StopConditionTextBox.Text, out var iterationStep))
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

        if (item.Content.Equals("Perceptron") && LearningRate != null)
        {
            InterfaceApp.Neuron = InterfaceApp.NEURON.Perceptron;
            LearningRate.Visibility = Visibility.Hidden;
            LearningRateLabel.Visibility = Visibility.Hidden;
        }

        if (item.Content.Equals("Adaline") && LearningRate != null)
        {
            InterfaceApp.Neuron = InterfaceApp.NEURON.Adaline;
            LearningRate.Visibility = Visibility.Visible;
            LearningRateLabel.Visibility = Visibility.Visible;
        }
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
        EnableElements(false);
        InterfaceApp.Solve();
        Plot();
        EnableElements(true);
    }

    private void LoadData_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Wczytaj dane"
        };
        openFileDialog.ShowDialog();
        InterfaceApp.LoadDataFromFile(openFileDialog.FileName);
    }

    private void NextStep_Click(object sender, RoutedEventArgs e)
    {
        Initialize();
        EnableElements(false);
        InterfaceApp.SolveStep();
        Plot();
    }

    private void Plot()
    {
        MainPlot.Plot.Clear();

        var data = InterfaceApp.Data;
        var inputsb = data.Where(x => x.Output == 1).Select(x => x.Input);
        var inputsr = data.Where(x => x.Output != 1).Select(x => x.Input);
        var input1b = inputsb.Select(x => (double)x[0]).ToArray();
        var input2b = inputsb.Select(x => (double)x[1]).ToArray();
        var input1r = inputsr.Select(x => (double)x[0]).ToArray();
        var input2r = inputsr.Select(x => (double)x[1]).ToArray();

        if (input1b.Length == input2b.Length && input1b.Length > 0)
            MainPlot.Plot.AddScatter(input1b, input2b, Color.Blue, 0);
        if (input1r.Length == input2r.Length && input1r.Length > 0)
            MainPlot.Plot.AddScatter(input1r, input2r, Color.Red, 0);

        var xs = new double[2];
        xs[0] = (double)data.Select(x => x.Input).Max(y => y[0]);
        xs[1] = (double)data.Select(x => x.Input).Min(y => y[0]);
        var xy = new double[2];

        xy[0] = (double)InterfaceApp.GetResultLinePoint((decimal)xs[0]);
        xy[1] = (double)InterfaceApp.GetResultLinePoint((decimal)xs[1]);

        MainPlot.Plot.AddScatter(xs, xy, color: Color.Green, markerSize: 0);

        MainPlot.Refresh();
    }

    private void EnableElements(bool state)
    {
        LoadData.IsEnabled = state;
        EditData.IsEnabled = state;
        Solve.IsEnabled = state;
        NeuronType.IsEnabled = state;
        StopCondition.IsEnabled = state;
        Load.IsEnabled = state;
        SaveAndExit.IsEnabled = state;
    }
}