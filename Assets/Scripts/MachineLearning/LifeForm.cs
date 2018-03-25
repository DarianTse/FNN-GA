using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Represents the entire lifeform, consisting of a neural network and a genotype
/// </summary>
public class LifeForm :  IComparable<LifeForm>{

    //Genotype of lifeform
    public Genotype GenoType { get; private set; }

    //Neural Network of lifeform
    public NeuralNetwork NN { get; private set; }

    //Is dead?
    private bool isAlive = false;

    //event for lifeform dying
    public event Action<LifeForm> LifeformDied; 

    /// <summary>
    /// Initialise new lifeform 
    /// </summary>
    /// <param name="_genotype">The genotype to use</param>
    /// <param name="layerSizes">The layerSizes of the neural network</param>
    public LifeForm(Genotype _genotype, params uint[] layerSizes)
    {
        isAlive = false;
        GenoType = _genotype;
        NN = new NeuralNetwork(layerSizes);

        //Check for matching number of genes to weights
        if (NN.WeightCount != _genotype.GeneCount)
        {
            throw new ArgumentException("Lifeform(): Genotype gene count does not match the number of weights in neural network");
        }

        IEnumerator<float> chromosome = _genotype.GetEnumerator();
        //Assign chromosome to weights in neural network
        foreach(NeuralLayer layer in NN.Layers)
        {
            for(int x = 0; x < layer.Weights.GetLength(0); x++) //Neurons of current layer
            {
                for(int y = 0; y < layer.Weights.GetLength(1); y++) //Neurons of next layer
                {
                    layer.Weights[x, y] = chromosome.Current;
                    chromosome.MoveNext();
                }
            }
        }
    }

    public void Reset()
    {
        GenoType.Fitness = 0;
        isAlive = true;
    }

    public void Kill()
    {
        isAlive = false;
        LifeformDied(this);
    }

    //IComparable Interface
    public int CompareTo(LifeForm other)
    {
        return GenoType.CompareTo(other.GenoType);
    }

}
