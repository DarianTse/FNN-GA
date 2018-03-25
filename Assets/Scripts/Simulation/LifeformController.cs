using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeformController : MonoBehaviour {

    //lifeform that this controller controls
    public LifeForm Lifeform { get; set; }
    
    public float CurrentFitness
    {
        get { return Lifeform.GenoType.Fitness; }
        set { Lifeform.GenoType.Fitness = value; }
    }

    //Health
    public float health;

    //Inputs for neural network : Resource positions
    float closestEnergyX;
    float closestEnergyY;
    float closestDangerX;
    float closestDangerY;

    //Inputs for neural network : Lifeform travel direction
    public Vector3 moveVec;

    //Angle lifeform is facing
    public double angle;

    //Turn speed
    const double MAX_TURN_SPEED = 0.3f;

    //lifeform variables
    const float HP_LOSS_OVER_TIME_VALUE = 1;
    const float HP_GAIN_VALUE = 2;
    const float HP_LOSS_VALUE = 1;
    const float HP_INITIAL = 5;
    const float HP_MAX = 5;

    //Evaluation variables
    int energyOrbsPicked, dangerOrbsPicked;
    public float timeAlive;
    private bool hasEvaluated;

    void Start()
    {
        health = HP_INITIAL;
        moveVec = Vector3.zero;
        angle = 0;
        energyOrbsPicked = 0;
        dangerOrbsPicked = 0;
        timeAlive = 0;
        hasEvaluated = false;
    }

    void FixedUpdate()
    {
        if (health > 0)
        {
            GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
            health -= HP_LOSS_OVER_TIME_VALUE * Time.fixedDeltaTime;
            timeAlive += Time.fixedDeltaTime;

            int[] closestResourceIndex = WorldController.Instance.GetClosestResourceIndex(transform.position);

            //Assign inputs
            closestEnergyX = WorldController.Instance.Resources[closestResourceIndex[0]].position.x;
            closestEnergyY = WorldController.Instance.Resources[closestResourceIndex[0]].position.y;
            closestDangerX = WorldController.Instance.Resources[closestResourceIndex[1]].position.x;
            closestDangerY = WorldController.Instance.Resources[closestResourceIndex[1]].position.y;


            //Angle between closest resource and lifeform
            //dot product of move vec and lifeform->closestEnergyResource vec 

            //float energyDotP = Vector3.Dot(moveVec, WorldController.Instance.Resources[closestResourceIndex[0]].position - transform.position);
            //float energyMagnitude = Vector3.Magnitude(WorldController.Instance.Resources[closestResourceIndex[0]].position - transform.position);
            //float moveMagnitude = Vector3.Magnitude(moveVec);
            //float energyAngle = MathFunctions.ToDegree(Mathf.Acos(energyDotP / (energyMagnitude * moveMagnitude)));

            float energyAngle = Vector3.Angle(moveVec, WorldController.Instance.Resources[closestResourceIndex[0]].position - transform.position); //RIP, shoulda read unity documentation first
            float dangerAngle = Vector3.Angle(moveVec, WorldController.Instance.Resources[closestResourceIndex[1]].position - transform.position);
          
            //Inputs to neural network 
            double[] inputs = {closestEnergyX, closestEnergyY, energyAngle,
                            closestDangerX, closestDangerY, dangerAngle,
                              moveVec.x, moveVec.y};

            //output values from the nn, outputs are left and right speed
            double[] nnOutputs = Lifeform.NN.CalculateOutputs(inputs);

            //Move lifeform according to outputs
            double angleChange = nnOutputs[0] - nnOutputs[1];
            if (angleChange < -MAX_TURN_SPEED) angleChange = -MAX_TURN_SPEED;
            if (angleChange > MAX_TURN_SPEED) angleChange = MAX_TURN_SPEED;

            //Update angle lifeform is facing
            angle += angleChange;
            double speed = nnOutputs[0] + nnOutputs[1];

            //Update move vector - get x and y towards angle direction as a unit vec
            //http://www.hitechnic.com/blog/wp-content/uploads/SinCosGraph.png
            moveVec.x = Mathf.Cos((float)angle);
            moveVec.y = Mathf.Sin((float)angle);

            //Update position
            transform.position += moveVec * (float)speed * Time.fixedDeltaTime;

            //Update out of bounds
            Vector3 pos = transform.position;
            WorldController.Instance.FixOutOfBounds(ref pos);
            transform.position = pos;

            //Check if lifeform can pickup resource
            int index = WorldController.Instance.ResourceOnPosition(transform.position);
            if (index >= 0)
            {
                switch (WorldController.Instance.Resources[index].GetComponent<ResourceType>().type)
                {
                    case ResourceType.Type.Energy:
                        health += HP_GAIN_VALUE;
                        if(health > HP_MAX) { health = HP_MAX; }
                        energyOrbsPicked += 1;
                        WorldController.Instance.RespawnResource(index);
                        Simulation.Instance.EnergyPicked += 1;
                        break;
                    case ResourceType.Type.Danger:
                        health -= HP_LOSS_VALUE;
                        dangerOrbsPicked += 1;
                        WorldController.Instance.RespawnResource(index);
                        Simulation.Instance.DangerPicked += 1;
                        break;
                }
            }
        }
        else
        {
            Die();
        }
    }

    public void Restart()
    {
        //reset variables
        health = HP_INITIAL;
        energyOrbsPicked = 0;
        dangerOrbsPicked = 0;
        timeAlive = 0;
        hasEvaluated = false;

        Lifeform.Reset();
        enabled = true;
    }

    public void Die()
    {
        Evaluate();
        
        enabled = false;
        Lifeform.Kill();
        GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
    }

    private void Evaluate()
    {
        if (!hasEvaluated)
        {
            float energyVal = Simulation.Instance.EnergyPicked > 0 ?
                energyOrbsPicked / Simulation.Instance.EnergyPicked : 0;
            float dangerVal = Simulation.Instance.DangerPicked > 0 ?
                dangerOrbsPicked / Simulation.Instance.DangerPicked : 0;

            float healthVal = health < 0 ?
                0 : health / HP_MAX;

            float timeVal = timeAlive > Simulation.Instance.GenerationDuration ?
                1 : timeAlive / Simulation.Instance.GenerationDuration;

            CurrentFitness = healthVal +
                timeVal + 
                energyVal - 
                dangerVal;

#if UNITY_EDITOR
            //print("Health :" + healthVal);
            //print("Time Alive: " + timeVal);
            //print("EnergyVal" + energyVal);
            //print("DangerVal" + dangerVal);
            //print("Fitness: " + CurrentFitness);
#endif 
            hasEvaluated = true;
        }
    }
}

