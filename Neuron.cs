namespace NeuronProject;

public abstract class Neuron
{
    public IList<decimal> Weights { get; init; } = new List<decimal>();
    public IList<decimal> Inputs { get; init; } = new List<decimal>();
    public decimal ExpectedOutput { get; set; }
    protected abstract decimal ActivationFunction(decimal x);

    public virtual decimal Calculate()
    {
        if (Weights.Count != Inputs.Count || !Weights.Any())
            throw new Exception("Weights.Count() != Inputs.Count() || !Weights.Any()");

        decimal sum = 0;

        for (var i = 0; i < Weights.Count; i++) sum += Weights[i] * Inputs[i];

        return ActivationFunction(sum);
    }

    public abstract void Learn();
}