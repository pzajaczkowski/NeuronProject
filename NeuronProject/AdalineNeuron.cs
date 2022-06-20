namespace NeuronProject;

public class AdalineNeuron : Neuron
{
    public decimal LearningRate { get; set; }

    protected override decimal ActivationFunction(decimal x)
    {
        return x > 0 ? 1 : 0;
    }

    public override void Learn()
    {
        var result = Calculate();

        if (result == ExpectedOutput)
            return;

        var sum = Weights[0] * Inputs[0] + Weights[1] * Inputs[1] + Bias;

        for (var i = 0; i < Weights.Count; i++) Weights[i] += LearningRate * (ExpectedOutput - sum) * Inputs[i];

        Bias += LearningRate * (ExpectedOutput - sum);
    }
}