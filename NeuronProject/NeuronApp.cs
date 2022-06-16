using System.Text.Json;

namespace NeuronProject;

public class NeuronApp
{
    private readonly List<Data> _data = new();
    private readonly List<decimal> _error = new();
    private readonly List<Data> _results = new();

    public NeuronApp()
    {
    }

    public NeuronApp(Neuron neuron, ulong iterations, List<Data> data, List<decimal> error, List<Data> result)
    {
        Neuron = neuron;
        Iterations = iterations;
        _data = data;
        _error = error;
        _results = result;
    }

    public Neuron Neuron { get; set; }

    public IList<Data> Data => _data.AsReadOnly();
    public IList<Data> Results => _results.AsReadOnly();
    public IList<decimal> AvgErrorList => _error.AsReadOnly();
    public decimal CurrentAvgError => _error.Last();

    public ulong Iterations { get; private set; }

    public void LoadDataFromDataList(List<Data> data)
    {
        _data.Clear();

        foreach (var item in data)
            AddData(item);
    }

    public void LoadDataFromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException();

        var json = File.ReadAllText(path);

        var data = JsonSerializer.Deserialize<List<Data>>(json);

        if (data == null)
            throw new Exception("json?");

        LoadDataFromDataList(data);
    }

    public void AddData(Data data)
    {
        if (data.Input.Count != 2)
            throw new Exception("data.Input.Count != 2");

        if (Neuron is PerceptronNeuron && data.Output is not (1 or -1))
            throw new Exception("Neuron allows only 1 or -1 as output");

        if (Neuron is AdalineNeuron && data.Output is not (1 or 0))
            throw new Exception("Neuron allows only 1 or 0 as output");

        _data.Add(data);
    }

    public void Calculate()
    {
        _results.Clear();

        foreach (var data in _data)
        {
            Neuron.Inputs = data.Input;

            _results.Add(new Data
            {
                Input = data.Input,
                Output = Neuron.Calculate()
            });
        }
    }

    public decimal CalculateWithAvgError()
    {
        Calculate();

        return AvgError();
    }

    private decimal AvgError()
    {
        decimal error = 0;

        for (var i = 0; i < _results.Count; i++)
            error += Math.Abs(_results[i].Output - _data[i].Output);

        return error / _data.Count;
    }

    public void Learn()
    {
        Iterations++;

        foreach (var data in _data)
        {
            Neuron.Inputs = data.Input;
            Neuron.ExpectedOutput = data.Output;

            Neuron.Learn();
        }

        _error.Add(CalculateWithAvgError());
    }

    public void Reset(Neuron neuron)
    {
        _error.Clear();
        _results.Clear();
        Neuron = neuron;
        Iterations = 0;
    }
}