namespace NeuronProject;

public class AdalineNeuron : Neuron
{
    public decimal LearningRate { get; init; } = new(.01);

    protected override decimal ActivationFunction(decimal x)
    {
        return x > 0 ? 1 : 0;
    }

    public override void Learn()
    {
        var result = Calculate();

        for (var i = 0; i < Weights.Count; i++) Weights[i] += LearningRate * (ExpectedOutput - result) * Inputs[i];

        Bias += LearningRate * (ExpectedOutput - result);
    }
}