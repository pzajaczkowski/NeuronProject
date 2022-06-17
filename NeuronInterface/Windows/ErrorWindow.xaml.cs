using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Color = System.Drawing.Color;

namespace NeuronInterface.Windows
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public bool TrueClose = false;

        public ErrorWindow()
        {
            InitializeComponent();
        }

        public void UpdateErrorPlot()
        {
            IList<decimal> errorList = InterfaceApp.AvgErrorList;
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
            this.Hide();
        }
    }
}
