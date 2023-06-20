using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
public class GameController : NetworkBehaviour
{
    [Header("Game Settings")]
    public int zombiesToSpawn = 10;
    public int spawnNewZombieTime = 5;
    int maxZombies = 20;
    [Space]
    public List<GameObject> spawnedZombies = new List<GameObject>();

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        Application.targetFrameRate = 144;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        //SpawnPlayer(OwnerClientId);

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            SpawnZombie();
        }

        InvokeRepeating("SpawnZombie", 15f, spawnNewZombieTime);
    }

    public void SpawnPlayer(ulong clientId)
    {
        GameObject tmpPlayer = Instantiate(Resources.Load("(BOT) Boss") as GameObject);
        tmpPlayer.transform.SetPositionAndRotation(Functions.GetRandomPositionWithinDistance(this.transform, 10f), new Quaternion(0, Random.Range(0, 200), 0, 0));
        tmpPlayer.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }

    void SpawnZombie()
    {
        CleanUpZombies();   // Could be done in a better way, but we just clean up the list before adding a new.

        // Spawn a new one, if the limit has not been reached.
        if (spawnedZombies.Count < maxZombies)
        {
            GameObject tmpZombie = Instantiate(Resources.Load("(BOT) Zombie") as GameObject);
            tmpZombie.transform.SetPositionAndRotation(Functions.GetRandomPositionWithinDistance(transform, 15f), new Quaternion(0, Random.Range(0, 200), 0, 0));
            tmpZombie.GetComponent<NetworkObject>().Spawn();
            spawnedZombies.Add(tmpZombie);
        }
    }

    private void CleanUpZombies()
    {
        // Loop through zombies & check if they are dead, then remove them from the list
        for (int i = 0; i < spawnedZombies.Count; i++)
        {
            if (spawnedZombies[i] == null)    // Zombie has been destroyed, remove it from the list.
                spawnedZombies.RemoveAt(i);
        }
    }
}
