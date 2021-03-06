﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Simulation : MonoBehaviour {

    public static Simulation Instance { get; private set; }

    //Lifeform prefab
    public GameObject lifeformPrefab;

    //Setup for population size and neural network size
    public int PopulationSize = 30;
    public uint[] NNLayersSize;

    //List of current lifeforms
    private List<LifeformController> lifeformControllers = new List<LifeformController>();
    private List<LifeForm> lifeforms = new List<LifeForm>();

    //Number of lifeforms alive
    public int LifeformsAliveCount { get; private set; }

    //The genetic algorithm
    private GeneticAlgorithm ga;

    //Event for all lifeforms dying
    public event Action AllLifeformsDead;

    public float GenerationDuration = 30.0f;
    public float TimeLeft { get; set; }
    public bool isCountingDown = false;

    public uint GenerationCount { get { return ga.GenerationCount; } }  
    public float HighestFitness { get { return ga.HighestFitness; } }

    //Danger resources picked up
    public int DangerPicked { get; set; }
    //Energy resources picked up
    public int EnergyPicked { get; set; }

    public GameObject DangerZone {
        get { return WorldController.Instance.DangerZone; }
        set { }
    }
    void Awake()
    {
        if(Instance != null)
        {
            print("More than one manager exists");
            return;
        }
        Instance = this;
    }

    public void Start()
    {
        StartSimulation();
    }

    public void FixedUpdate()
    {
        if (isCountingDown)
            TimeLeft -= Time.fixedDeltaTime;
        if (TimeLeft <= 0)
        {
            EvaluateAllLifeforms();
            KillAllLifeforms();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCountingDown = !isCountingDown;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            EvaluateAllLifeforms();
            KillAllLifeforms();
        }

        if (Input.GetKey("up"))
            DangerZone.transform.position = new Vector3(DangerZone.transform.position.x, DangerZone.transform.position.y + 0.1f, 0.0f);

        if (Input.GetKey("down"))
            DangerZone.transform.position = new Vector3(DangerZone.transform.position.x, DangerZone.transform.position.y - 0.1f, 0.0f);

        if (Input.GetKey("left"))
            DangerZone.transform.position = new Vector3(DangerZone.transform.position.x - 0.1f, DangerZone.transform.position.y, 0.0f);

        if (Input.GetKey("right"))
            DangerZone.transform.position = new Vector3(DangerZone.transform.position.x + 0.1f, DangerZone.transform.position.y, 0.0f);
    }

    public void StartSimulation()
    {
        //Create neural network with proper Layer Sizes
        NeuralNetwork nn = new NeuralNetwork(NNLayersSize);

        //Setup genetic algorithm
        ga = new GeneticAlgorithm(nn.WeightCount, (uint)PopulationSize);
        ga.StartGeneration = CallInitLifeforms;
        AllLifeformsDead += ga.EndGeneration;
        ga.Start();

#if UNITY_EDITOR
        lifeforms.ForEach(lifeform => print(lifeform.NN.GetNetworkString()));
#endif
    }

    private void CallInitLifeforms(List<Genotype> currentPopulation)
    {
        StartCoroutine(InitialiseLifeforms(currentPopulation));
    }

    //Create new lifeforms using the genotype population from the genetic algorithm
    private IEnumerator InitialiseLifeforms(List<Genotype> currentPopulation)
    {
        yield return new WaitForFixedUpdate();
        lifeforms.Clear();
        LifeformsAliveCount = 0;
        DangerPicked = 0;
        EnergyPicked = 0;
        TimeLeft = GenerationDuration;

        //Create list of lifeforms with genotypes in current population
        currentPopulation.ForEach(genotype => lifeforms.Add(new LifeForm(genotype, NNLayersSize)));

        //instantiate lifeforms first time, otherwise randomize position 
        if (lifeformControllers.Count < lifeforms.Count)
        {
            for (int i = 0; i < lifeforms.Count; i++)
            {
                GameObject newLifeform = Instantiate(lifeformPrefab, WorldController.Instance.GetRandomPositionOutsideDangerZone(), Quaternion.identity);
                lifeformControllers.Add(newLifeform.GetComponent<LifeformController>());
            }
        }
        else
        {
            lifeformControllers.ForEach(lifeform => lifeform.transform.position = WorldController.Instance.GetRandomPositionOutsideDangerZone());
        }

        //Link up lifeform controllers and lifeforms
        for (int i = 0; i < lifeforms.Count; i++)
        {
            lifeformControllers[i].GetComponent<LifeformController>().Lifeform = lifeforms[i];
            lifeformControllers[i].GetComponent<LifeformController>().Restart();
            LifeformsAliveCount++;
            lifeforms[i].LifeformDied += OnLifeformDied;
        }


    }

    public void EvaluateAllLifeforms()
    {
        lifeformControllers.ForEach(c => c.Evaluate());
    }

    //Kill all lifeforms
    private void KillAllLifeforms()
    {
        foreach (LifeformController c in lifeformControllers)
        {
            if (c.enabled)
            {
                c.Die();
            }
        }
    }

    //LifeformDied callback
    private void OnLifeformDied(LifeForm lifeform)
    {
        LifeformsAliveCount--;
        if(LifeformsAliveCount == 0 && AllLifeformsDead != null)
        {
            AllLifeformsDead();
        }
    }

}
