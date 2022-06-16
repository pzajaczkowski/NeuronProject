using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace NeuronInterface.Windows;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InterfaceApp.StateChangedEvent += StateChangedEvent;
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
        //EnableElements(false);
        InterfaceApp.Solve();
        Plot();
        //EnableElements(true);
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
        //EnableElements(false);
        InterfaceApp.SolveStep();

        UpdateText();
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

        var ((x, y), (x1, y1)) = InterfaceApp.GetLine();
        MainPlot.Plot.AddLine(x, y, x1, y1, Color.Green);
        MainPlot.Refresh();
    }

    private void StopSolve_Click(object sender, RoutedEventArgs e)
    {
        InterfaceApp.Stop();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        InterfaceApp.Reset();
        MainPlot.Plot.Clear();
        MainPlot.Refresh();
        Reset.IsEnabled = false;
    }

    private void StateChangedEvent(object? sender, InterfaceApp.STATE e)
    {
        UpdateText();

        switch (e)
        {
            case InterfaceApp.STATE.Empty:
                LoadData.IsEnabled = true;
                EditData.IsEnabled = true;
                Solve.IsEnabled = false;
                StopSolve.IsEnabled = false;
                Reset.IsEnabled = false;
                NextStep.IsEnabled = false;
                NeuronType.IsEnabled = false;
                StopCondition.IsEnabled = false;
                LearningRate.IsEnabled = false;
                Load.IsEnabled = true;
                SaveAndExit.IsEnabled = true;
                break;
            case InterfaceApp.STATE.Stopped:
                LoadData.IsEnabled = false;
                EditData.IsEnabled = false;
                Solve.IsEnabled = true;
                StopSolve.IsEnabled = false;
                Reset.IsEnabled = true;
                NextStep.IsEnabled = true;
                NeuronType.IsEnabled = false;
                StopCondition.IsEnabled = false;
                LearningRate.IsEnabled = false;
                Load.IsEnabled = true;
                SaveAndExit.IsEnabled = true;
                break;
            case InterfaceApp.STATE.Waiting:
                LoadData.IsEnabled = true;
                EditData.IsEnabled = true;
                Solve.IsEnabled = true;
                StopSolve.IsEnabled = false;
                Reset.IsEnabled = false;
                NextStep.IsEnabled = true;
                NeuronType.IsEnabled = true;
                StopCondition.IsEnabled = true;
                LearningRate.IsEnabled = true;
                Load.IsEnabled = true;
                SaveAndExit.IsEnabled = true;
                break;
            case InterfaceApp.STATE.Running:
                LoadData.IsEnabled = false;
                EditData.IsEnabled = false;
                Solve.IsEnabled = false;
                StopSolve.IsEnabled = true;
                Reset.IsEnabled = false;
                NextStep.IsEnabled = false;
                NeuronType.IsEnabled = false;
                StopCondition.IsEnabled = false;
                LearningRate.IsEnabled = false;
                Load.IsEnabled = false;
                SaveAndExit.IsEnabled = false;
                break;
            default:
                throw new Exception("On state changed event");
        }
    }

    private void UpdateText()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        NeuronType.SelectedIndex = (int)InterfaceApp.Neuron;

        StopConditionTextBox.Text = InterfaceApp.Mode == InterfaceApp.MODE.Error
            ? InterfaceApp.MaxError.ToString(culture)
            : InterfaceApp.IterationStep.ToString();
        Iteration.Text = InterfaceApp.Iteration.ToString();

        if (InterfaceApp.Neuron == InterfaceApp.NEURON.Adaline)
            LearningRate.Text = InterfaceApp.LearningRate.ToString(culture);

        Iteration.Text = InterfaceApp.Iteration.ToString();
        CurrentError.Text = InterfaceApp.AvgError.ToString(culture);

        //Plot();
    }

    private void SaveAndExit_Click(object sender, RoutedEventArgs e)
    {
        var FileDialog = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Wczytaj dane"
        };
        FileDialog.ShowDialog();

        Initialize();
        InterfaceApp.SaveToJson(FileDialog.FileName);
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        var FileDialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Wczytaj dane"
        };
        FileDialog.ShowDialog();
        InterfaceApp.LoadFromJson(FileDialog.FileName);

        UpdateText();
    }
}