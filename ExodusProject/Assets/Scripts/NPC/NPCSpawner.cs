using UnityEngine;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject npcPrefab;
    public int spawnCount = 10;
    public float spawnRadius = 50f;
    
    [Header("Auto-Configure")]
    public bool assignWanderCenter = true;

    private List<GameObject> _spawnedNPCs = new List<GameObject>();

    void Start()
    {
        SpawnNPCs();
    }

    public void SpawnNPCs()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC Spawner: No NPC Prefab assigned!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity, transform);
            
            if (assignWanderCenter)
            {
                NPCWanderer wanderer = npc.GetComponent<NPCWanderer>();
                if (wanderer != null)
                {
                    wanderer.wanderRadius = spawnRadius;
                }
            }

            _spawnedNPCs.Add(npc);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
        randomPos += transform.position;
        randomPos.y = transform.position.y; // Keep at spawner height

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return transform.position; // Fallback to spawner position
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
