using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;

    void Start()
    {
        SpawnObstacle();
        
    }

    // Update is called once per frame
    void Update()
    {
           
    }
    
    public void SpawnObstacle(){
        int obstacleSpawnIndex = Random.Range(2,5);
        Transform spawnPoint = transform.GetChild(obstacleSpawnIndex).transform;
        Quaternion spawnRotation = transform.GetChild(obstacleSpawnIndex).rotation;

        Instantiate(obstaclePrefab,spawnPoint.position,spawnRotation,transform);
    }
}
