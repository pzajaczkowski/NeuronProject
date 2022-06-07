namespace NeuronProject;

internal abstract class Neuron
{
    public IList<decimal> Weights { get; init; } = new List<decimal>();
    public IList<decimal> Inputs { get; set; } = new List<decimal>();
    public decimal Bias { get; init; }
    public decimal ExpectedOutput { get; set; }
    protected abstract decimal ActivationFunction(decimal x);

    /// <summary>
    ///     Oblicza wyjście neuronu
    /// </summary>
    /// <returns> Wyjście neuronu </returns>
    /// <exception cref="Exception"></exception>
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