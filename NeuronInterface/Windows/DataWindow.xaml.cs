using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using NeuronProject;

namespace NeuronInterface.Windows;

/// <summary>
///     Interaction logic for DataWindow.xaml
/// </summary>
public partial class DataWindow : Window
{
    private readonly InterfaceApp _interfaceApp;

    private ObservableCollection<DataItem> _dataGridCollection;

    public DataWindow(InterfaceApp interfaceApp)
    {
        _interfaceApp = interfaceApp;
        InitializeComponent();
        CreateDataGridData();
    }

    private void CreateDataGridData()
    {
        _dataGridCollection = new ObservableCollection<DataItem>();

        foreach (var data in _interfaceApp.Data)
        {
            var item = new DataItem
            {
                Input1 = data.Input[0].ToString(),
                Input2 = data.Input[1].ToString(),
                Output = data.Output.ToString()
            };
            _dataGridCollection.Add(item);
        }

        DataGrid.ItemsSource = _dataGridCollection;
    }

    private void Return_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = DataGrid.SelectedItems;

        var list = new List<DataItem>();

        foreach (var selectedItem in selectedItems)
            try
            {
                var item = (DataItem)selectedItem;
                list.Add(item);
            }
            catch (InvalidCastException)
            {
            }

        foreach (var selectedItem in list.ToList())
            _dataGridCollection.Remove(selectedItem);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var datalist = new List<Data>();

        foreach (var dataItem in _dataGridCollection)
        {
            var data = new Data();

            if (decimal.TryParse(dataItem.Input1, out var dec1) &&
                decimal.TryParse(dataItem.Input2, out var dec2) &&
                decimal.TryParse(dataItem.Output, out var dec3))
            {
                data.Input = new List<decimal> { dec1, dec2 };
                data.Output = dec3;

                datalist.Add(data);
            }
            else
            {
                MessageBox.Show("Błędnie wpisane dane!");
                Close();
            }
        }

        _interfaceApp.LoadDataFromDataList(datalist);
        Close();
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex("[^0-9,-]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    //tymczasowe rozwiazanie
    private class DataItem
    {
        public string Input1 { get; init; }
        public string Input2 { get; init; }
        public string Output { get; init; }
    }
}