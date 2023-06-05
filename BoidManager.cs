using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    [Range(2,25)]
    public int boidCount = 10;

    public GameObject boid;

    private List<GameObject> boids = new List<GameObject>();

    GRNG grng = new GRNG();

    public bool resetBoids = false;

    // Start is called before the first frame update
    void Start()
    {
        boids = new List<GameObject>();


        SpawnAllBoids();
    }

    void SpawnBoid()
    {
        var newBoid = Instantiate(boid);
        newBoid.SetActive(false);
        newBoid.transform.position = Vector3.zero;
        newBoid.transform.position += new Vector3(grng.Range(-8, 8),0, grng.Range(-8, 8));
        newBoid.transform.parent = this.transform;
        newBoid.SetActive(true);
        boids.Add(newBoid);
    }


    void SpawnAllBoids()
    {
        boids = new List<GameObject>();

        for (int i = 0; i < boidCount; i++)
        {
            SpawnBoid();
        }
    }


    private void RespawnBoids()
    {
        CustomUtilities.DestoryChildren(this.gameObject);
        SpawnAllBoids();
    }


    private void Update()
    {
        if(resetBoids == true)
        {
            RespawnBoids();
            resetBoids = false;
        }
    }
}
