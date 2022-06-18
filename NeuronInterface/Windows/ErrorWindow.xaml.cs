using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace NeuronInterface.Windows;

/// <summary>
///     Interaction logic for ErrorWindow.xaml
/// </summary>
public partial class ErrorWindow : Window
{
    private readonly InterfaceApp _interfaceApp;
    public bool TrueClose = false;

    public ErrorWindow(InterfaceApp interfaceApp)
    {
        _interfaceApp = interfaceApp;
        InitializeComponent();
    }

    public void UpdateErrorPlot()
    {
        var errorList = _interfaceApp.AvgErrorList;
        if (errorList.Count == 0)
        {
            ErrorPlot.Plot.Clear();
            ErrorPlot.Refresh();
            return;
        }

        var xy = errorList.Select(Convert.ToDouble).ToArray();
        var xs = Enumerable.Range(0, xy.Length).Select(Convert.ToDouble).ToArray();
        ErrorPlot.Plot.Clear();
        ErrorPlot.Plot.AddScatter(xs, xy, Color.Orange);
        ErrorPlot.Refresh();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (TrueClose) return;
        e.Cancel = true;
        Hide();
    }
}