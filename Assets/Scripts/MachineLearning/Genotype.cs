using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// https://www.tutorialspoint.com/genetic_algorithms/genetic_algorithms_genotype_representation.htm
/// Represent 1 member of a population using Real Valued Representation
/// Used to determine weights of the NN, also contains the fitness value 
/// </summary>
public class Genotype : IComparable<Genotype>, IEnumerable<float>
{

    //Fitness of this genotype
    public float Fitness { get; set; }

    //In this case, chromosome represents the weights of the neural network
    public float[] Chromosome { get; private set; }

    //Number of values in chromosome
    public int GeneCount
    {
        get { if (Chromosome == null) return 0; return Chromosome.Length; }
    }

    public Genotype(float[] _chromosome)
    {
        Chromosome = _chromosome;
        Fitness = 0;
    }


    /// <summary>
    /// Set the genes of the chromosome to random values
    /// </summary>
    public void SetRandomGenes()
    {
        for (int i = 0; i < Chromosome.Length; i++)
        {
            Chromosome[i] = (float)MathFunctions.GetRandom(GlobalData.MinGeneValue, GlobalData.MaxGeneValue);
        }
    }

    //IComparable Interface
    public int CompareTo(Genotype other)
    {
        return other.Fitness.CompareTo(Fitness);
    }

    //IEnumerable Interface iterating over genes in the chromosome
    public IEnumerator<float> GetEnumerator()
    {
        foreach (float gene in Chromosome)
        {
            yield return gene; 
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (float gene in Chromosome)
        {
            yield return gene;
        }
    }

}
