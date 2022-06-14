using NeuronProject;

namespace NeuronInterface;

internal class InterfaceApp
{
    public enum MODE
    {
        Error,
        Iterations
    }

    public MODE Mode { get; set; } = MODE.Error;

    public NeuronApp NeuronApp { get; private set; }
}