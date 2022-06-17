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
        Running,

        /// <summary>
        ///     Stan błędu. Wyświetla komunikat z treścią błedu.
        /// </summary>
        Error,

        /// <summary>
        ///     Percepton nauczony. Brak możliwości dalszego uczenia.
        /// </summary>
        Finished
    }

    // Początkowe stany
    private static MODE _mode = MODE.Error;
    private static NEURON _neuron = NEURON.Perceptron;
    private static STATE _state = STATE.Empty;

    public static EventHandler<STATE>? StateChangedEvent;

    /// <summary>
    ///     Używane do automatycznego rozpoznawania typu neuronu podczas wczytywania danych.
    /// </summary>
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
            _neuron = value;

            _state = STATE.Empty;
            ResetData();

            NeuronApp.Neuron = value switch
            {
                NEURON.Perceptron => new PerceptronNeuron
                {
                    Bias = NeuronApp.Neuron.Bias,
                    Weights = NeuronApp.Neuron.Weights
                },
                NEURON.Adaline => new AdalineNeuron
                {
                    Bias = NeuronApp.Neuron.Bias,
                    Weights = NeuronApp.Neuron.Weights
                },
                _ => throw new Exception("how?????")
            };
        }
    }

    private static void ResetData()
    {
        NeuronApp.ClearData();
    }

    /// <summary>
    ///     Opis błędu. Używać, gdy State == Error.
    /// </summary>
    public static string ErrorMessage { get; private set; } = string.Empty;

    /// <summary>
    ///     Warunek zatrzymania. Maksymalny błąd neuronu.
    /// </summary>
    public static decimal MaxError { get; set; }

    /// <summary>
    ///     Warunek zatrzymania. Ilość iteracji.
    /// </summary>
    public static ulong IterationStep { get; set; }

    public static decimal LearningRate { get; set; } = new(.1);
    private static NeuronApp NeuronApp { get; set; } = new() { Neuron = new PerceptronNeuron() };

    /// <summary>
    ///     Lista z danymi do nauczania neuronu.
    /// </summary>
    public static IList<Data> Data => NeuronApp.Data;

    /// <summary>
    ///     Numer aktualnej iteracji.
    /// </summary>
    public static ulong Iteration => NeuronApp.Iterations;

    /// <summary>
    ///     Aktualny bład neuronu.
    /// </summary>
    public static decimal AvgError => NeuronApp.CurrentAvgError;

    /// <summary>
    ///     Lista średniego błędu dla każdej iteracji.
    /// </summary>
    public static IList<decimal> AvgErrorList => NeuronApp.AvgErrorList;

    public static void Stop()
    {
        State = STATE.Stopped;
    }

    /// <summary>
    ///     Odnajduje dwa punkty, które oznaczają linię, która oddziela zbiory punktów.
    /// </summary>
    /// <returns>
    ///     (Punkt1(x, y), Punkt2(x, y))
    /// </returns>
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
        if (State is STATE.Running or STATE.Stopped or STATE.Error)
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
        catch (FileNotFoundException)
        {
        }
        catch (JsonException e)
        {
            ErrorMessage = "Niepoprawny format danych";
            State = STATE.Error;
            Console.WriteLine(e);
        }
        catch (Exception e)
        {
            if (_loadingTry > 1)
            {
                ErrorMessage = "Niepoprawny format danych";
                _loadingTry = 0;
                return;
            }

            switch (e)
            {
                case NeuronApp.AdalineDataException:
                    Neuron = NEURON.Perceptron;
                    break;
                case NeuronApp.PerceptronDataException:
                    Neuron = NEURON.Adaline;
                    break;
                default:
                    ErrorMessage = "Niepoprawny format danych";
                    _loadingTry = 0;
                    return;
            }

            LoadDataFromFile(path);
            return;
        }

        _loadingTry = 0;
    }

    /// <summary>
    ///     Wykonuje jedną iterację nauczania.
    /// </summary>
    public static void SolveStep()
    {
        State = STATE.Running;
        if (Neuron == NEURON.Adaline)
        {
            var neuron = (AdalineNeuron)NeuronApp.Neuron;
            neuron.LearningRate = LearningRate;
        }

        NeuronApp.Learn();

        State = STATE.Stopped;
    }

    /// <summary>
    ///     Wykonuje nauczanie do osiągnięcia podanego warunku zatrzymania.
    /// </summary>
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
                if (!SolveWithMaxError())
                {
                    State = STATE.Stopped;
                    ErrorMessage = $"Nie udało się znaleźć rozwiązania po {Iteration} iteracjach";
                    State = STATE.Error;
                    return;
                }

                break;
            case MODE.Iterations:
                SolveWithIterationStep();
                break;
            default:
                throw new Exception("how?");
        }

        State = AvgError > 0 ? STATE.Stopped : STATE.Finished;
    }

    private static bool SolveWithMaxError()
    {
        ulong it = 0;
        while (NeuronApp.CalculateWithAvgError() > MaxError)
        {
            NeuronApp.Learn();

            if (++it == 100000)
                return false;
        }

        return true;
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

    /// <summary>
    ///     Zapisuje stan całej aplikacji pliku.
    /// </summary>
    /// <param name="path">ścieżka do pliku</param>
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

    /// <summary>
    ///     Wczytuje stan całej aplikacji z pliku.
    /// </summary>
    /// <param name="path">ścieżka do pliku</param>
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

            if (app.NeuronApp == null)
                throw new JsonException();

            _mode = app.Mode;
            _neuron = app.NeuronMode;
            MaxError = app.MaxError;
            IterationStep = app.IterationStep;
            LearningRate = app.LearningRate;
            NeuronApp = new NeuronApp(app.NeuronApp.Neuron, app.NeuronApp.Iterations, app.NeuronApp.Data,
                app.NeuronApp.Error, app.NeuronApp.Results);
        }
        catch (FileNotFoundException)
        {
        }
        catch (Exception e)
        {
            if (e is not (JsonException or NullReferenceException or JsonSerializationException))
                throw;

            ErrorMessage = "Niepoprawny format danych";
            State = STATE.Error;
            return;
        }

        if (AvgError == 0)
        {
            State = STATE.Finished;
            return;
        }

        if (Iteration > 0)
        {
            State = STATE.Stopped;
            return;
        }

        State = STATE.Waiting;
    }
}

// Klasy do wczytywania całej aplikacji
public static partial class InterfaceApp
{
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