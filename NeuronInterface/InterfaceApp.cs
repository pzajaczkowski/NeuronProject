﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeuronProject;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace NeuronInterface;

public static partial class InterfaceApp
{
    public static void SaveToJson(string path)
    {
        if (path == string.Empty)
            return;

        var app = new AppJson
        {
            IterationStep = IterationStep,
            LearningRate = LearningRate,
            MaxError = MaxError,
            NeuronApp = new NeuronAppJson
            {
                Data = NeuronApp.Data.ToList(),
                Error = NeuronApp.AvgErrorList.ToList(),
                Results = NeuronApp.Results.ToList(),
                Neuron = NeuronApp.Neuron,
                Iterations = NeuronApp.Iterations
            },
            NeuronMode = _neuron,
            Mode = _mode
        };

        var json = JsonConvert.SerializeObject(app, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        File.WriteAllText(path, json);
    }

    public static void LoadFromJson(string path)
    {
        if (path == string.Empty)
            return;

        try
        {
            var json = File.ReadAllText(path);
            var app = JsonConvert.DeserializeObject<AppJson>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            _mode = app.Mode;
            _neuron = app.NeuronMode;
            MaxError = app.MaxError;
            IterationStep = app.IterationStep;
            LearningRate = app.LearningRate;
            NeuronApp = new NeuronApp(app.NeuronApp.Neuron, app.NeuronApp.Iterations, app.NeuronApp.Data,
                app.NeuronApp.Error, app.NeuronApp.Results);

            State = STATE.Waiting;
        }
        catch (FileNotFoundException e)
        {
            //TODO
            Console.WriteLine(e);
        }
        catch (JsonException e)
        {
            //TODO
            Console.WriteLine(e);
        }
    }

    private class NeuronAppJson
    {
        public List<Data> Data { get; init; }
        public List<decimal> Error { get; init; }
        public List<Data> Results { get; init; }
        public Neuron Neuron { get; init; }
        public ulong Iterations { get; init; }
    }

    private class AppJson
    {
        public MODE Mode { get; init; }
        public NEURON NeuronMode { get; init; }
        public NeuronAppJson NeuronApp { get; init; }
        public decimal MaxError { get; init; }
        public ulong IterationStep { get; init; }
        public decimal LearningRate { get; init; }
    }
}

public static partial class InterfaceApp
{
    /// <summary>
    ///     Tryb uczenia neuronu, warunek zatrzymania się uczenia.
    /// </summary>
    public enum MODE
    {
        /// <summary>
        ///     Warunek zatrzymania maksymalny bład.
        /// </summary>
        Error,

        /// <summary>
        ///     Warunek zatrzymania ilość iteracji.
        /// </summary>
        Iterations
    }

    /// <summary>
    ///     Typ neuronu.
    /// </summary>
    public enum NEURON
    {
        Perceptron,
        Adaline
    }

    /// <summary>
    ///     Aktualny stan aplikacji.
    /// </summary>
    public enum STATE
    {
        /// <summary>
        ///     Oczekiwania na dodanie danych.
        /// </summary>
        Empty,

        /// <summary>
        ///     Oczekiwania na pierwszą iterację uczenia się.
        /// </summary>
        Waiting,

        /// <summary>
        ///     Uczenie zatrzymane.
        /// </summary>
        Stopped,

        /// <summary>
        ///     Stan uczenia. Brak możliwości wykonywania akcji poza próby zatrzymania.
        /// </summary>
        Running
    }

    private static MODE _mode = MODE.Error;
    private static NEURON _neuron = NEURON.Perceptron;
    private static STATE _state = STATE.Empty;

    public static EventHandler<STATE>? StateChangedEvent;

    private static int _loadingTry;

    public static STATE State
    {
        get => _state;
        private set
        {
            _state = value;
            StateChangedEvent?.Invoke(null, _state);
        }
    }

    public static MODE Mode
    {
        get => _mode;
        set
        {
            if (State == STATE.Running)
                throw new Exception("Nie można zmienić trybu w czasie działania aplikacji");

            _mode = value;
        }
    }

    public static NEURON Neuron
    {
        get => _neuron;
        set
        {
            if (Running)
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

    public static bool Running => State != STATE.Waiting && State != STATE.Empty;
    private static NeuronApp NeuronApp { get; set; } = new() { Neuron = new PerceptronNeuron() };

    public static decimal MaxError { get; set; }
    public static ulong IterationStep { get; set; }
    public static decimal LearningRate { get; set; } = new(.1);
    public static IList<Data> Data => NeuronApp.Data;

    public static ulong Iteration => NeuronApp.Iterations;
    public static decimal AvgError => NeuronApp.CurrentAvgError;

    public static void Stop()
    {
        State = STATE.Stopped;
    }

    public static ((double, double), (double, double)) GetLine()
    {
        var min = NeuronApp.Data.Aggregate((min, item) => item.Input[0] < min.Input[0] ? item : min);
        var max = NeuronApp.Data.Aggregate((max, item) => item.Input[0] > max.Input[0] ? item : max);

        return (
            (
                (double)min.Input[0], (double)GetResultLinePoint(min.Input[0])
            ),
            (
                (double)max.Input[0], (double)GetResultLinePoint(max.Input[0])
            )
        );
    }

    private static decimal GetResultLinePoint(decimal x)
    {
        return -1 * (NeuronApp.Neuron.Bias + NeuronApp.Neuron.Weights[0] * x) / NeuronApp.Neuron.Weights[1];
    }

    public static void LoadDataFromDataList(List<Data> data)
    {
        if (State != STATE.Waiting)
            throw new Exception("Nie można wczytać danych w czasie działania aplikacji"); // xd

        NeuronApp.LoadDataFromDataList(data);
    }

    public static void LoadDataFromFile(string path)
    {
        _loadingTry++;
        try
        {
            NeuronApp.LoadDataFromFile(path);

            State = STATE.Waiting;
        }
        catch (FileNotFoundException e)
        {
            //TODO
            Console.WriteLine(e);
        }
        catch (JsonException e)
        {
            //TODO
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            if (_loadingTry > 1)
                throw;

            if (e is NeuronApp.AdalineDataException)
                Neuron = NEURON.Perceptron;
            if (e is NeuronApp.PerceptronDataException)
                Neuron = NEURON.Adaline;
            else
                throw;

            LoadDataFromFile(path);
            return;
        }

        _loadingTry = 0;
    }

    public static void SolveStep()
    {
        State = STATE.Running;

        NeuronApp.Learn();

        State = STATE.Stopped;
    }

    public static void Solve()
    {
        State = STATE.Running;
        if (Neuron == NEURON.Adaline)
        {
            var neuron = (AdalineNeuron)NeuronApp.Neuron;
            neuron.LearningRate = LearningRate;
        }

        switch (Mode)
        {
            case MODE.Error:
                SolveWithMaxError();
                break;
            case MODE.Iterations:
                SolveWithIterationStep();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        State = STATE.Stopped;
    }

    private static void SolveWithMaxError()
    {
        while (NeuronApp.CalculateWithAvgError() > MaxError)
            NeuronApp.Learn();
    }

    private static void SolveWithIterationStep()
    {
        for (ulong i = 0; i < IterationStep; i++)
            NeuronApp.Learn();
    }

    public static void Reset()
    {
        NeuronApp.Reset(Neuron == NEURON.Perceptron ? new PerceptronNeuron() : new AdalineNeuron());
        State = STATE.Waiting;
    }
}