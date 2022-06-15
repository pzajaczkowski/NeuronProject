using NeuronProject;
using System;
using System.Collections.Generic;

namespace NeuronInterface;

public class InterfaceApp
{
    public enum MODE
    {
        Error,
        Iterations
    }

    public enum NEURON
    {
        Perceptron,
        Adaline
    }

    public enum STATE
    {
        Waiting,
        Stopped,
        Running
    }

    private MODE _mode = MODE.Error;
    private NEURON _neuron = NEURON.Perceptron;

    public STATE State { get; } = STATE.Waiting;

    public MODE Mode
    {
        get => _mode;
        set
        {
            if (State == STATE.Running)
                throw new Exception("Nie można zmienić trybu w czasie działania aplikacji");

            _mode = value;
        }
    }

    public NEURON Neuron
    {
        get => _neuron;
        set
        {
            if (State == STATE.Running)
                throw new Exception("Nie można zmienić neuronu w czasie działania aplikacji");

            _neuron = value;

            NeuronApp.Neuron = value switch
            {
                NEURON.Perceptron => new PerceptronNeuron(),
                NEURON.Adaline => new AdalineNeuron(),
                _ => throw new Exception("how?????")
            };
        }
    }

    private NeuronApp NeuronApp { get; } = new() { Neuron = new PerceptronNeuron() };

    public void LoadDataFromDataList(List<Data> data)
    {
        if (State != STATE.Waiting)
            throw new Exception("Nie można wczytać danych w czasie działania aplikacji"); // xd

        NeuronApp.LoadDataFromDataList(data);
    }
}