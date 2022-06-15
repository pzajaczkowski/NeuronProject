using NeuronProject;
using System.Collections.Generic;
using System.Windows;

namespace NeuronInterface.Windows
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        //tymczasowe rozwiazanie
        public IList<Data> _data = new List<Data>();
        public DataWindow()
        {
            InitializeComponent();
        }

        private void CreateDataGridData()
        {

        }
    }
}
