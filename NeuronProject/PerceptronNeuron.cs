namespace NeuronProject;

public class PerceptronNeuron : Neuron
{
    protected override decimal ActivationFunction(decimal x)
    {
        return x > 0 ? 1 : -1;
    }

    public override void Learn()
    {
        var result = Calculate();

        if (result == ExpectedOutput)
            return;

        for (var i = 0; i < Weights.Count; i++)
            Weights[i] += Inputs[i] * ExpectedOutput;

        Bias += ExpectedOutput;
    }
}