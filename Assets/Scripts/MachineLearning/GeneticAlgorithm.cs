using System;
using System.Collections.Generic;
using System.Linq;

public class GeneticAlgorithm{

    private const float CrossoverRate = 0.7f;
    private const float GenotypeMutationRate = 1;
    private const float GeneMutationRate = 0.3f;
    private const float GeneMutationAmount = 2;

    public delegate void StartGenerationDelegate(List<Genotype> currentPopulation);
    public StartGenerationDelegate StartGeneration; 

    public bool Running { get; private set; }

    private List<Genotype> currentPopulation;

    //Amount of genotypes in population
    public uint PopulationSize { get; private set; }

    //Generations passed
    public uint GenerationCount { get; private set; }

    //Highest fitness
    public int HighestFitness { get; set; }

    /// <summary>
    /// Initialise new genetic algorithm, filling currentPopulation with genotypes
    /// with chromosome of size geneCount. 
    /// </summary>
    /// <param name="geneCount">Amount of genes in a Chromosome (number of weights)</param>
    /// <param name="populationSize">Size of population</param>
    public GeneticAlgorithm(uint geneCount, uint populationSize)
    {
        PopulationSize = populationSize;

        //Initialise genotypes with empty array of size geneCount
        currentPopulation = new List<Genotype>((int)populationSize);
        for (int i = 0; i < populationSize; i++)
        {
            Genotype newGenotype = new Genotype(new float[geneCount]);
            currentPopulation.Add(newGenotype);
        }

        HighestFitness = 0;
        GenerationCount = 1;
        Running = false;
    }

    public void Start()
    {
        Running = true;

        //initialize population with random values
        InitializePopulation(currentPopulation);

        //Call start generation from simulation
        if (StartGeneration != null)
        {
            StartGeneration(currentPopulation);
        }
    }

    public void EndGeneration()
    {
        CalculateFitness(currentPopulation);

        //Sort population with highest fitness at the front
        currentPopulation.Sort();

        //Select best population
        List<Genotype> bestPopulation = SelectBestPopulation(currentPopulation);

        //Create new generation
        List<Genotype> newPopulation = Recombination(bestPopulation, PopulationSize);

        //Mutation
        MutatePopulation(newPopulation);

        currentPopulation = newPopulation;
        GenerationCount++;

        StartGeneration(currentPopulation);
    }

    /// <summary>
    /// Initialises population with random values 
    /// </summary>
    /// <param name="population">List of genotypes to be initalized</param>
    public void InitializePopulation(List<Genotype> population)
    {
        population.ForEach(genotype => genotype.SetRandomGenes());
    }


    public void CalculateFitness(List<Genotype> population)
    {
    }

    /// <summary>
    /// Selection function to get the best
    /// </summary>
    /// <param name="population">The population to select from</param>
    /// <returns>2 best population</returns>
    public List<Genotype> SelectBestPopulation(List<Genotype> population)
    {
        List<Genotype> bestPopulation = new List<Genotype>();
        HighestFitness = (int)population[0].Fitness;
        bestPopulation.Add(population[0]);
        bestPopulation.Add(population[1]);

        return bestPopulation;
    }

    /// <summary>
    /// Returns a population created from using crossover on the 2 best population members
    /// </summary>
    /// <param name="bestPopulation">The best members of a generation</param>
    /// <param name="populationSize">The size of the new generation</param>
    /// <returns>A population created by using the crossover function on the best population of previous generation</returns>
    public List<Genotype> Recombination(List<Genotype> bestPopulation, uint populationSize)
    {
        if(bestPopulation.Count < 2)
        {
            throw new ArgumentException("bestPopulation list needs to contain at least 2 genotypes");
        }

        List<Genotype> newPopulation = new List<Genotype>();
        while (newPopulation.Count < populationSize)
        {
            Genotype child1, child2;
            Crossover(bestPopulation[0], bestPopulation[1], CrossoverRate, out child1, out child2);

            newPopulation.Add(child1);
            if(newPopulation.Count < populationSize)
            {
                newPopulation.Add(child2);
            }
        }
        return newPopulation;
    }

    /// <summary>
    /// Uniform Crossover function, assigns 2 child genotypes by reference 
    /// https://www.tutorialspoint.com/genetic_algorithms/genetic_algorithms_crossover.htm
    /// </summary>
    public void Crossover(Genotype parent1, Genotype parent2, float crossoverRate, out Genotype child1, out Genotype child2)
    {
        int geneCount = parent1.GeneCount;
        float[] child1Chromosome = new float[geneCount];
        float[] child2Chromosome = new float[geneCount];

        for (int i = 0; i < geneCount; i++)
        {
            if(MathFunctions.GetRandom(0,1) > crossoverRate)
            {
                //swap
                child1Chromosome[i] = parent2.Chromosome[i];
                child2Chromosome[i] = parent1.Chromosome[i];
            }
            else
            {
                //no swap
                child1Chromosome[i] = parent1.Chromosome[i];
                child2Chromosome[i] = parent2.Chromosome[i];
            }
        }

        child1 = new Genotype(child1Chromosome);
        child2 = new Genotype(child2Chromosome);
    }

    /// <summary>
    /// Mutate all genotypes in the population
    /// </summary>
    /// <param name="population">The population to mutate</param>
    public void MutatePopulation(List<Genotype> population)
    {
        foreach (Genotype genotype in population)
        {
            if (MathFunctions.GetRandom(0, 1) < GenotypeMutationRate)
            {
                MutateGenotype(genotype, GeneMutationRate, GeneMutationAmount);
            }
        }
    }

    /// <summary>
    /// Mutate genotype by adding or subtracting a random value from each gene if probability condition passes 
    /// </summary>
    public void MutateGenotype(Genotype genotype, float geneMutationRate, float mutationAmount)
    {
        for (int i = 0; i < genotype.GeneCount; i++)
        {
            if(MathFunctions.GetRandom(0,1) < geneMutationRate)
            {
                //mutate
                genotype.Chromosome[i] += (float)(MathFunctions.GetRandom(0, 1) * (mutationAmount * 2) - mutationAmount);
            }
        }
    }

}
