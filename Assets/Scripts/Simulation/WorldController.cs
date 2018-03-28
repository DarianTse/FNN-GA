using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour {

    //Instance
    public static WorldController Instance;

    //The X and Y boundaries of the camera
    public float BoundaryX, BoundaryY;

    //Danger zone reference -> Assigned in inspector
    public GameObject DangerZone;

    //Prefab
    public GameObject energyPrefab;
    public GameObject dangerPrefab;

    //Total resources spawned on map (Spawns with a ratio of 2:1 energy:danger)
    public int resourcesCount = 10;

    //Array of resources
    public Transform[] Resources { get; private set; }

    const float RESOURCE_PICKUP_RADIUS = 0.22f;


    void Awake()
    {
        if (Instance != null)
        {
            print("More than one World Controller exists");
            return;
        }
        Instance = this;

    }

    //Get camera values
	void Start () {
        BoundaryY = Camera.main.orthographicSize;
        BoundaryX = BoundaryY * Screen.width / Screen.height;

        ResetResources();
	}

    /// <summary>
    /// Check if parameter pos is inside the danger zone
    /// </summary>
    /// <param name="pos">The position to compare</param>
    /// <returns>true if inside</returns>
    public bool WithinDangerZone(Vector3 pos)
    {
        if(DangerZone.GetComponent<Collider>().bounds.Contains(pos))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    ///  Clear energy resources array. Respawn them in random positions
    /// </summary>
    private void ResetResources()
    {
        Resources = new Transform[resourcesCount];
        for (int i = 0; i < resourcesCount; i++)
        {
            Vector3 position = GetRandomPosition();
            if ((i + 3) % 3 == 0)
            {
                Resources[i] = Instantiate(dangerPrefab, position, Quaternion.identity).transform;
                Resources[i].name = dangerPrefab.name;
            }
            else
            {
                Resources[i] = Instantiate(energyPrefab, position, Quaternion.identity).transform;
                Resources[i].name = energyPrefab.name;
            }
        }
    }

    /// <summary>
    /// Check if there is a resource at position and return the index of that resource
    /// </summary>
    /// <param name="position">The position to check</param>
    /// <returns>Index of the resource on position</returns>
    public int ResourceOnPosition(Vector3 position)
    {
        for(int i = 0; i < resourcesCount; i++)
        {
            float distance = Vector3.Distance(position, Resources[i].position);
            if(distance <= RESOURCE_PICKUP_RADIUS)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Randomize energy position at index in array
    /// </summary>
    /// <param name="index">index of resource in array</param>
    public void RespawnResource(int index)
    {
        Resources[index].transform.position = GetRandomPosition();
    }

    /// <summary>
    /// Returns the index of the closest energy [0] and closest danger[1]
    /// </summary>
    /// <param name="lifeformPos">The lifeform to compare from</param>
    /// <returns></returns>
    public int[] GetClosestResourceIndex(Vector3 lifeformPos)
    {
        int nearestEnergyIndex = -1;
        int nearestDangerIndex = -1;
        float nearestEnergyDistance = 0;
        float nearestDangerDistance = 0;

        //Find the nearest energy and danger resource
        for(int i = 0; i < resourcesCount; i++)
        {
            float distance = Vector3.Distance(lifeformPos, Resources[i].position);
            switch(Resources[i].GetComponent<ResourceType>().type)
            {
                case ResourceType.Type.Danger:
                    if ((distance < nearestDangerDistance) || nearestDangerIndex == -1)
                    {
                        nearestDangerIndex = i;
                        nearestDangerDistance = distance;
                    }
                    break;
                case ResourceType.Type.Energy:
                    if ((distance < nearestEnergyDistance) || nearestEnergyIndex == -1)
                    {
                        nearestEnergyIndex = i;
                        nearestEnergyDistance = distance;
                    }
                    break;
            }
        }
        int[] output = { nearestEnergyIndex, nearestDangerIndex };
        return output;
    }


    public Vector3 GetRandomPosition()
    {
        Vector3 position = new Vector3((float)MathFunctions.GetRandom(-BoundaryX, BoundaryX),
                                          (float)MathFunctions.GetRandom(-BoundaryY, BoundaryY),
                                          0);
        return position;
    }

    public Vector3 GetRandomPositionOutsideDangerZone()
    {
        //Get the ratio of the dangerzone position relative to the boundaries. 
        float ratioX = (DangerZone.transform.position.x + BoundaryX) / (BoundaryX * 2);
        float ratioY = (DangerZone.transform.position.y + BoundaryY) / (BoundaryY * 2);

        float val = Random.Range(0.0f,1.0f);
        //Get a random value on the left side of danger zone if random value is less than ratioX, right side if greater
        float randomX = val < ratioX ?
            Random.Range(-BoundaryX, -DangerZone.GetComponent<Renderer>().bounds.extents.x) :
            Random.Range(DangerZone.GetComponent<Renderer>().bounds.extents.x, BoundaryX);
        val = Random.Range(0.0f, 1.0f);
        //Get a random value below the danger zone if random value is less than ratioY, above if greater
        float randomY = val < ratioY ?
            Random.Range(-BoundaryY, -DangerZone.GetComponent<Renderer>().bounds.extents.y) :
            Random.Range(DangerZone.GetComponent<Renderer>().bounds.extents.y, BoundaryY);

        return new Vector3(randomX, randomY, 0);
    }

    //Check if position is out of bounds and rectify it
    public void FixOutOfBounds(ref Vector3 pos)
    {
        if(pos.x > BoundaryX) { pos.x = -BoundaryX; }
        if(pos.x < -BoundaryX) { pos.x = BoundaryX; }
        if(pos.y > BoundaryY) { pos.y = -BoundaryY; }
        if(pos.y < -BoundaryY) { pos.y = BoundaryY;  }
    }
}
