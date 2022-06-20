using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using NeuronProject;
using Color = System.Drawing.Color;

namespace NeuronInterface.Windows;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ErrorWindow _errorWindow;
    private readonly InterfaceApp _interfaceApp;

    public MainWindow()
    {
        _interfaceApp = new InterfaceApp();

        InitializeComponent();
        _interfaceApp.StateChangedEvent += StateChangedEvent;
        _errorWindow = new ErrorWindow(_interfaceApp);
    }

    private void PositiveNumber(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex(@"^\d*\.?\d*$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void Initialize()
    {
        if (decimal.TryParse(LearningRate.Text, NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"),
                out var learningRate))
            _interfaceApp.LearningRate = learningRate;

        switch (_interfaceApp.Mode)
        {
            case InterfaceApp.MODE.Error:
            {
                if (!decimal.TryParse(StopConditionTextBox.Text, NumberStyles.AllowDecimalPoint,
                        CultureInfo.GetCultureInfo("en-US"), out var maxError))
                    throw new Exception();

                _interfaceApp.MaxError = maxError;
                break;
            }
            case InterfaceApp.MODE.Iterations:
            {
                if (!ulong.TryParse(StopConditionTextBox.Text, out var iterationStep))
                    throw new Exception();

                _interfaceApp.IterationStep = iterationStep;
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void EditData_Click(object sender, RoutedEventArgs e)
    {
        var dataWindow = new DataWindow(_interfaceApp) { Owner = this };
        dataWindow.ShowDialog();
    }

    private void NeuronType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = (ComboBoxItem)e.AddedItems[0];

        if (LearningRate != null &&
            MessageBox.Show("Zmiana neuronu powoduje usunięcie danych.\n Czy chcesz kontynuować?", "Uwaga!",
                MessageBoxButton.YesNo) == MessageBoxResult.No) return;

        if (item.Content.Equals("Perceptron") && LearningRate != null)
        {
            _interfaceApp.Neuron = InterfaceApp.NEURON.Perceptron;
            LearningRate.Visibility = Visibility.Hidden;
            LearningRateLabel.Visibility = Visibility.Hidden;
        }

        if (item.Content.Equals("Adaline") && LearningRate != null)
        {
            _interfaceApp.Neuron = InterfaceApp.NEURON.Adaline;
            LearningRate.Visibility = Visibility.Visible;
            LearningRateLabel.Visibility = Visibility.Visible;
        }
    }

    private void StopCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = (ComboBoxItem)e.AddedItems[0];

        if (item.Content.Equals("Próg błędu"))
            _interfaceApp.Mode = InterfaceApp.MODE.Error;

        if (item.Content.Equals("Ilość iteracji"))
            _interfaceApp.Mode = InterfaceApp.MODE.Iterations;
    }

    private void Solve_Click(object sender, RoutedEventArgs e)
    {
        Initialize();
        _interfaceApp.SolveAsync(Updater);
    }

    private void Updater(IList<Data> data)
    {
        Plot(data);
        _errorWindow.UpdateErrorPlot();

        Iteration.Text = _interfaceApp.Iteration.ToString();
        CurrentError.Text = _interfaceApp.AvgError.ToString(CultureInfo.GetCultureInfo("en-US"));
    }

    private void LoadData_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Wczytaj dane"
        };
        openFileDialog.ShowDialog();
        _interfaceApp.LoadDataFromFile(openFileDialog.FileName);
    }

    private void NextStep_Click(object sender, RoutedEventArgs e)
    {
        Initialize();
        _interfaceApp.SolveStep();
    }

    private void Plot(IList<Data> data)
    {
        MainPlot.Plot.Clear();

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

        var ((x, y), (x1, y1)) = _interfaceApp.GetLine();
        MainPlot.Plot.AddLine(x, y, x1, y1, Color.Green);
        MainPlot.Refresh();
    }

    private void StopSolve_Click(object sender, RoutedEventArgs e)
    {
        _interfaceApp.Stop();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _interfaceApp.Reset();
    }

    private void StateChangedEvent(object? sender, InterfaceApp.STATE e)
    {
        UpdateTextAndPlot();

        switch (e)
        {
            case InterfaceApp.STATE.Empty:
                LoadData.IsEnabled = true;
                EditData.IsEnabled = true;
                Solve.IsEnabled = false;
                StopSolve.IsEnabled = false;
                Reset.IsEnabled = false;
                NextStep.IsEnabled = false;
                ErrorGraph.IsEnabled = true;
                NeuronType.IsEnabled = false;
                StopCondition.IsEnabled = false;
                StopConditionTextBox.IsEnabled = false;
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
                ErrorGraph.IsEnabled = true;
                NeuronType.IsEnabled = false;
                StopCondition.IsEnabled = false;
                StopConditionTextBox.IsEnabled = false;
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
                ErrorGraph.IsEnabled = true;
                NeuronType.IsEnabled = true;
                StopCondition.IsEnabled = true;
                StopConditionTextBox.IsEnabled = true;
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
                ErrorGraph.IsEnabled = true;
                NeuronType.IsEnabled = false;
                StopCondition.IsEnabled = false;
                StopConditionTextBox.IsEnabled = false;
                LearningRate.IsEnabled = false;
                Load.IsEnabled = false;
                SaveAndExit.IsEnabled = false;
                break;
            case InterfaceApp.STATE.Finished:
                LoadData.IsEnabled = false;
                EditData.IsEnabled = false;
                Solve.IsEnabled = false;
                StopSolve.IsEnabled = false;
                Reset.IsEnabled = true;
                NextStep.IsEnabled = false;
                ErrorGraph.IsEnabled = true;
                NeuronType.IsEnabled = false;
                StopCondition.IsEnabled = false;
                StopConditionTextBox.IsEnabled = false;
                LearningRate.IsEnabled = false;
                Load.IsEnabled = true;
                SaveAndExit.IsEnabled = true;
                break;
            case InterfaceApp.STATE.Error:
                break;
            default:
                throw new Exception("On state changed event");
        }
    }

    private void UpdateTextAndPlot()
    {
        switch (_interfaceApp.State)
        {
            case InterfaceApp.STATE.Error:
                ErrorMessage.Content = _interfaceApp.ErrorMessage;
                ErrorMessage.Foreground = new SolidColorBrush(new System.Windows.Media.Color { A = 255, R = 255 });
                return;
            case InterfaceApp.STATE.Finished:
                ErrorMessage.Content = "Znalezione rozwiązanie";
                ErrorMessage.Foreground = new SolidColorBrush(new System.Windows.Media.Color { A = 255, G = 255 });
                break;
            default:
                ErrorMessage.Content = string.Empty;
                break;
        }

        var culture = CultureInfo.GetCultureInfo("en-US");
        NeuronType.SelectedIndex = (int)_interfaceApp.Neuron;

        StopConditionTextBox.Text = _interfaceApp.Mode == InterfaceApp.MODE.Error
            ? _interfaceApp.MaxError.ToString(culture)
            : _interfaceApp.IterationStep.ToString();

        Iteration.Text = _interfaceApp.Iteration.ToString();

        if (_interfaceApp.Neuron == InterfaceApp.NEURON.Adaline)
            LearningRate.Text = _interfaceApp.LearningRate.ToString(culture);

        Iteration.Text = _interfaceApp.Iteration.ToString();

        if (_interfaceApp.State == InterfaceApp.STATE.Empty)
        {
            CurrentError.Text = "-";
            return;
        }

        CurrentError.Text = _interfaceApp.AvgError.ToString(culture);

        _errorWindow.UpdateErrorPlot();
        Plot(_interfaceApp.Data);
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
        _interfaceApp.SaveToJson(FileDialog.FileName);
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        var FileDialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Title = "Wczytaj dane"
        };
        FileDialog.ShowDialog();
        _interfaceApp.LoadFromJson(FileDialog.FileName);
    }

    private void ErrorGraph_Click(object sender, RoutedEventArgs e)
    {
        _errorWindow.Show();
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        _errorWindow.TrueClose = true;
        _errorWindow.Close();
    }
}