using System.Text.Json;

namespace NeuronProject;

public class NeuronApp
{
    private readonly List<Data> _results = new();
    private List<Data> _data = new();
    public Neuron? Neuron { get; set; }

    public IList<Data> Data => _data.AsReadOnly();
    public IList<Data> Results => _results.AsReadOnly();
    public int Iterations { get; private set; }

    public void LoadDataFromDataList(List<Data> data)
    {
        _data = data;
    }

    public void LoadDataFromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException();

        var json = File.ReadAllText(path);

        _data.Clear();

        _data = JsonSerializer.Deserialize<List<Data>>(json);
    }

    public void AddData(Data data)
    {
        if (data.Input.Count != 2)
            throw new Exception("data.Input.Count != 2");

        if (_neuron is PerceptronNeuron && data.Output is not (1 or -1))
            throw new Exception("Neuron allows only 1 or -1 as output");

        if (_neuron is AdalineNeuron && data.Output is not (1 or 0))
            throw new Exception("Neuron allows only 1 or 0 as output");

        _data.Add(data);
    }

    public void Calculate()
    {
        _results.Clear();

        foreach (var data in _data)
        {
            _neuron.Inputs = data.Input;

            _results.Add(new Data
            {
                Input = data.Input,
                Output = _neuron.Calculate()
            });
        }
    }

    public decimal AvgError()
    {
        decimal error = 0;

        Calculate();

        for (var i = 0; i < _results.Count; i++)
            error += Math.Abs(_results[i].Output - _data[i].Output);

        return error / _data.Count;
    }

    public void Learn()
    {
        Iterations++;

        foreach (var data in _data)
        {
            _neuron.Inputs = data.Input;
            _neuron.ExpectedOutput = data.Output;

            _neuron.Learn();
        }
    }
}