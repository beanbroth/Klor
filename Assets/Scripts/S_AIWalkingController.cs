using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class S_AIWalkingController : MonoBehaviour
{
    [SerializeField] private Transform target;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (target == null)
        {
            Debug.LogError("Target is not assigned to the S_AIWalkingController!");
        }
    }

    void Update()
    {
        if (target != null && agent != null)
        {
            agent.SetDestination(target.position);
        }
    }
}