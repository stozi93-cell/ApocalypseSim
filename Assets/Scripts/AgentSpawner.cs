using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public int agentCount = 40;

    void Start()
    {
        AgentMovement firstAgent = null;

        for (int i = 0; i < agentCount; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(-8f, 8f),
                Random.Range(-4.5f, 4.5f)
            );

            GameObject obj = Instantiate(agentPrefab, pos, Quaternion.identity);

            if (i == 0)
                firstAgent = obj.GetComponent<AgentMovement>();
        }

        // FORCE ONE PANIC SEED
        if (firstAgent != null)
{
    Debug.Log("FORCING PANIC");
    firstAgent.ForcePanic();
}
    }
}
