namespace NeuronProject;

public abstract class Neuron
{
    public IList<decimal> Weights { get; init; } = new List<decimal>();
    public IList<decimal> Inputs { get; set; } = new List<decimal>();

    public decimal Bias { get; set; }
    public decimal ExpectedOutput { get; set; }

    protected void SetRandomWeights()
    {
        var random = new Random();

        Weights.Add(new decimal(random.NextDouble()));
        Weights.Add(new decimal(random.NextDouble()));

        Bias = new decimal(random.NextDouble());
    }

    protected abstract decimal ActivationFunction(decimal x);

    public virtual decimal Calculate()
    {
        if (Weights.Count != Inputs.Count || !Weights.Any())
            SetRandomWeights();
        //throw new Exception("Weights.Count() != Inputs.Count() || !Weights.Any()");

        decimal sum = 0;

        for (var i = 0; i < Weights.Count; i++) sum += Weights[i] * Inputs[i];

        return ActivationFunction(sum + Bias);
    }

    public abstract void Learn();
}