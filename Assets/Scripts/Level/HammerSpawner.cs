using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class HammerSpawner : NetworkBehaviour
{
    public GameObject hammer;
    public float minSpawnInterval = 30f;
    public float randomnessFactor = 30f;

    private float nextSpawnTime;
    
    private void Start() {
        nextSpawnTime = generateNextSpawnTime();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(IsServer) {
            if(Time.time >= nextSpawnTime) {
                GlobalGameManager.Instance.GameManager.spawnHammerServerRpc(transform.position, Quaternion.Euler(0f, Random.Range(0,365), 0f));
                nextSpawnTime = generateNextSpawnTime();
            }
        }
    }

    private float generateNextSpawnTime() {
        return Time.time + minSpawnInterval + Random.Range(0f, randomnessFactor);
    }
}
