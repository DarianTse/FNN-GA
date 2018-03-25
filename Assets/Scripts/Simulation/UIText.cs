using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIText : MonoBehaviour {

    Text text; 

	void Start () {
        text = transform.GetComponent<Text>();	
	}

	void Update () {
        text.text = "Generation: " + Simulation.Instance.GenerationCount.ToString() + "\n" +
                    "Lifeforms Alive: " + Simulation.Instance.LifeformsAliveCount.ToString() + "\n" +
                    "Highest Fitness: " + Simulation.Instance.HighestFitness.ToString() + "\n" +
                    "Danger Resources Picked: " + Simulation.Instance.DangerPicked.ToString() + "\n" +
                    "Energy Resources Picked: " + Simulation.Instance.EnergyPicked.ToString(); 
	}
}
