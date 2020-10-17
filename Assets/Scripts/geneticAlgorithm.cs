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
        if (currentGeneration < population.Length - 1)
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
