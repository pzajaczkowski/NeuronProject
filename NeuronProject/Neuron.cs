namespace NeuronProject;

public abstract class Neuron
{
    protected Neuron()
    {
        var random = new Random();

        Weights = new List<decimal>();
        Weights.Add(new decimal(random.NextDouble()));
        Weights.Add(new decimal(random.NextDouble()));

        Bias = new decimal(random.NextDouble());
    }

    public IList<decimal> Weights { get; }
    public IList<decimal> Inputs { get; set; } = new List<decimal>();

    public decimal Bias { get; protected set; }
    public decimal ExpectedOutput { get; set; }
    protected abstract decimal ActivationFunction(decimal x);

    public virtual decimal Calculate()
    {
        if (Weights.Count != Inputs.Count || !Weights.Any())
            throw new Exception("Weights.Count() != Inputs.Count() || !Weights.Any()");

        decimal sum = 0;

        for (var i = 0; i < Weights.Count; i++) sum += Weights[i] * Inputs[i];

        return ActivationFunction(sum + Bias);
    }

    public abstract void Learn();
}