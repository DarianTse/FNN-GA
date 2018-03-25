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
    const float HP_LOSS_OVER_TIME_VALUE = 1;
    const float HP_GAIN_VALUE = 20;
    const float HP_LOSS_VALUE = 100;
    const float HP_INITIAL = 500;

    void Start()
    {
        health = HP_INITIAL;
        moveVec = Vector3.zero;
        angle = 0;
    }

    void FixedUpdate()
    {
        if (health > 0)
        {
            GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
            health -= HP_LOSS_OVER_TIME_VALUE;

            int[] closestResourceIndex = WorldController.Instance.GetClosestResourceIndex(transform.position);

            //Assign inputs
            closestEnergyX = WorldController.Instance.Resources[closestResourceIndex[0]].position.x;
            closestEnergyY = WorldController.Instance.Resources[closestResourceIndex[0]].position.y;
            closestDangerX = WorldController.Instance.Resources[closestResourceIndex[1]].position.x;
            closestDangerY = WorldController.Instance.Resources[closestResourceIndex[1]].position.y;

            //Inputs to neural network 
            double[] inputs = {closestEnergyX, closestEnergyY,
                            closestDangerX, closestDangerY,
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
            transform.position += moveVec * (float)speed * 0.1f;

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
                        CurrentFitness += 1;
                        WorldController.Instance.RespawnResource(index);
                        Simulation.Instance.EnergyPicked += 1;
                        break;
                    case ResourceType.Type.Danger:
                        health -= HP_LOSS_VALUE;
                        CurrentFitness -= 5;
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
        health = HP_INITIAL;
        Lifeform.Reset();
        enabled = true;
    }

    private void Die()
    {
        enabled = false;
        Lifeform.Kill();
        GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
    }
}
