using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;

public class geneticAlgorithm : MonoBehaviour
{
    [Header("References")]
    public CarController controller;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = .055f;

    [Header("CrossoverControls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;

    public int numberToCrossover;

    List<int> genePool = new List<int>();
    int naturallySelected;
    NeuralNetwork[] population;

    [Header("Debugging")]
    public int currentGeneration;
    public int currentGenome = 0;

    // Start is called before the first frame update
    void Start()
    {
        createPopulation();
    }

    private void createPopulation()
    {
        population = new NeuralNetwork[initialPopulation];
        randomFillPopulation(population, 0);
        resetToCurrentGenome();
    }

    private void randomFillPopulation(NeuralNetwork[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new NeuralNetwork();
            newPopulation[startingIndex].initialize(controller.layers, controller.neurons);
            startingIndex++;
        }
    }

    public void death(float fitness, NeuralNetwork network)
    {
        if (currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            resetToCurrentGenome();
        }
        else
        {
            repopulate();
        }
    }

    void repopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        sortPopulation();

        NeuralNetwork[] newPopulation = pickBestPopulation();

        crossover(newPopulation);
        mutate(newPopulation);
        randomFillPopulation(newPopulation, naturallySelected);

        population = newPopulation;
        currentGenome = 0;
        resetToCurrentGenome();
    }

    private void mutate(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            for (int j = 0; j < newPopulation[i].weights.Count; j++)
            {
                if (UnityEngine.Random.Range(0.0f,1.0f)<mutationRate)
                {
                    newPopulation[i].weights[j] = mutateMatrix(newPopulation[i].weights[j]);
                }
            }
        }
    }

    private Matrix<float> mutateMatrix(Matrix<float> A)
    {
        int randomPoints = UnityEngine.Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = UnityEngine.Random.Range(0, C.ColumnCount);
            int randomRow = UnityEngine.Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + UnityEngine.Random.Range(-1f, 1f), -1f, 1f);
        }

        return C;
    }

    private void crossover(NeuralNetwork[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            int Aindex = i;
            int Bindex = i + 1;

            if (genePool.Count>=1)
            {
                for (int k = 0; k < 100; k++)
                {
                    Aindex = genePool[UnityEngine.Random.Range(0, genePool.Count)];
                    Bindex = genePool[UnityEngine.Random.Range(0, genePool.Count)];
                    if (Aindex!=Bindex)
                    {
                        break;
                    }
                }
            }

            NeuralNetwork child1 = new NeuralNetwork();
            NeuralNetwork child2 = new NeuralNetwork();

            child1.initialize(controller.layers, controller.neurons);
            child2.initialize(controller.layers, controller.neurons);

            child1.fitness = 0;
            child2.fitness = 0;

            for (int j = 0; j < child1.weights.Count; j++)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.weights[j] = population[Aindex].weights[j];
                    child2.weights[j] = population[Bindex].weights[j];
                }
                else
                {
                    child2.weights[j] = population[Aindex].weights[j];
                    child1.weights[j] = population[Bindex].weights[j];
                }
            }
            for (int j = 0; j < child1.biases.Count; j++)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    child1.biases[j] = population[Aindex].biases[j];
                    child2.biases[j] = population[Bindex].biases[j];
                }
                else
                {
                    child2.biases[j] = population[Aindex].biases[j];
                    child1.biases[j] = population[Bindex].biases[j];
                }
            }

            newPopulation[naturallySelected] = child1;
            naturallySelected++;

            newPopulation[naturallySelected] = child2;
            naturallySelected++;
        }
    }

    private NeuralNetwork[] pickBestPopulation()
    {
        NeuralNetwork[] newPopulation = new NeuralNetwork[initialPopulation];

        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].initializeCopy(controller.layers, controller.neurons);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int j = 0; j < f; j++)
            {
                genePool.Add(i);
            }
        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int j = 0; j < f; j++)
            {
                genePool.Add(last);
            }
        }

        return newPopulation;
    }

    void sortPopulation()
    {
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = 0; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    NeuralNetwork temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
    }

    private void resetToCurrentGenome()
    {
        controller.resetWithNetwork(population[currentGenome]);
    }
}
