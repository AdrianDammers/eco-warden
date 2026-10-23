using UnityEngine;
using UnityEngine.AI;
using ithappy.Animals_FREE;

public class AnimalFSM : MonoBehaviour
{
    public enum State { Wander, Flee }
    public State currentState = State.Wander;

    [Header("References")]
    public NavMeshAgent agent;
    public CreatureMover mover;
    public Transform player;

    [Header("Parameters")]
    public float fleeDistance = 7.0f;
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;

    private float timer;

    void Start()
    {
        // Remove the asset's conflicting input script to avoid console errors
        var playerInputScript = GetComponent<MovePlayerInput>();
        if (playerInputScript != null)
        {
            Destroy(playerInputScript);
        }

        // Automatically assign components if left empty in the inspector
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (mover == null) mover = GetComponent<CreatureMover>();

        timer = wanderTimer;

        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = true;
        }

        // Automatically find the player using the "Player" tag
        FindPlayer();
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- STATE MACHINE LOGIC ---
        if (distanceToPlayer < fleeDistance)
        {
            currentState = State.Flee;
        }
        else
        {
            currentState = State.Wander;
        }

        Vector3 targetPosition = transform.position;
        bool isRunning = false;

        switch (currentState)
        {
            case State.Wander:
                if (agent != null) agent.speed = 1.5f;
                timer += Time.deltaTime;
                if (timer >= wanderTimer)
                {
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                    if (agent != null) agent.SetDestination(newPos);
                    timer = 0;
                }
                if (agent != null) targetPosition = agent.steeringTarget;
                isRunning = false;
                break;

            case State.Flee:
                if (agent != null) agent.speed = 4.5f;

                // Calculate safe escape direction away from the player
                Vector3 runDir = player.position - transform.position;
                if (runDir.sqrMagnitude < 0.01f)
                {
                    runDir = -transform.forward;
                }

                Vector3 fleePos = transform.position - runDir.normalized * 7.0f;

                NavMeshHit navHit;
                NavMesh.SamplePosition(fleePos, out navHit, 7.0f, -1);
                if (agent != null) agent.SetDestination(navHit.position);

                if (agent != null) targetPosition = agent.steeringTarget;
                isRunning = true;
                break;
        }

        if (agent != null)
        {
            agent.nextPosition = transform.position;
        }

        Vector3 worldDelta = targetPosition - transform.position;
        worldDelta.y = 0f;
        Vector3 localDir = transform.InverseTransformDirection(worldDelta.normalized);
        Vector2 inputAxis = new Vector2(localDir.x, localDir.z);

        if (mover != null && worldDelta.magnitude > 0.2f)
        {
            mover.SetInput(inputAxis, targetPosition, isRunning, false);
        }
        else if (mover != null)
        {
            mover.SetInput(Vector2.zero, transform.position + transform.forward, false, false);
        }
    }

    void FindPlayer()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            player = pObj.transform;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}