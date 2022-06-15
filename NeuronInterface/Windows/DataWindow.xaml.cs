using NeuronProject;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeuronInterface.Windows
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        //tymczasowe rozwiazanie
        public class DataItem
        {
            public string Input1 { get; set; }
            public string Input2 { get; set; }
            public string Output { get; set; }
        }

        private readonly InterfaceApp app;
        public ObservableCollection<DataItem> DataGridCollection;
        public DataWindow(InterfaceApp _app)
        {
            InitializeComponent();
            app = _app;
            CreateDataGridData();
        }

        private void CreateDataGridData()
        {
            DataGridCollection = new ObservableCollection<DataItem>();
            foreach (var data in app.GetData())
            {
                var item = new DataItem
                {
                    Input1 = data.Input[0].ToString(),
                    Input2 = data.Input[1].ToString(),
                    Output = data.Output.ToString()
                };
                DataGridCollection.Add(item);
            }
            DataGrid.ItemsSource = DataGridCollection;
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DataGridCollection.Remove((DataItem)DataGrid.SelectedItem);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var datalist = new List<Data>();
            foreach (var dataItem in DataGridCollection)
            {
                var data = new Data();
                var tmp = new List<decimal>();
                if (decimal.TryParse(dataItem.Input1, out var dec1) && decimal.TryParse(dataItem.Input2, out var dec2) && decimal.TryParse(dataItem.Output, out var dec3))
                {
                    tmp.Add(dec1);
                    tmp.Add(dec2);
                    data.Input = tmp;
                    data.Output = dec3;
                    datalist.Add(data);
                }
                else
                {
                    MessageBox.Show("Błędnie wpisane dane!");
                    this.Close();
                }
            }
            app.LoadDataFromDataList(datalist);
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,-]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
