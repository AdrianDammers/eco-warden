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
    public AnimalPerception perception;
    public Transform player;

    [Header("Parameters")]
    public float safeDistance = 15.0f;  // stop fleeing when the player is further away than this...
    public float minFleeTime = 4.0f;    // ...and the animal has fled for at least this long
    public float fleeStep = 7.0f;       // how far ahead each flee point is picked
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;

    private float timer;
    private float fleeTimer;

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
        if (perception == null) perception = GetComponent<AnimalPerception>();

        timer = wanderTimer;

        if (agent != null)
        {
            // The agent only plans the path. CreatureMover moves and rotates the animal.
            agent.updatePosition = false;
            agent.updateRotation = false;
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

        // --- SENSE: ask the perception script ---
        // (single | so HeardShot() is always called and resets, even when the player is seen)
        bool noticedPlayer = perception != null && (perception.CanSeePlayer() | perception.HeardShot());

        // --- THINK: state machine logic ---
        if (noticedPlayer)
        {
            // Seeing or hearing the player always (re)starts the flight
            currentState = State.Flee;
            fleeTimer = 0f;
        }
        else if (currentState == State.Flee)
        {
            fleeTimer += Time.deltaTime;
            // Calm down only when far enough away AND after fleeing for a while
            if (distanceToPlayer > safeDistance && fleeTimer > minFleeTime)
            {
                currentState = State.Wander;
                timer = wanderTimer; // pick a new wander point right away
            }
        }

        bool isRunning = false;

        switch (currentState)
        {
            case State.Wander:
                if (agent != null) agent.speed = 1.5f;
                timer += Time.deltaTime;
                if (timer >= wanderTimer)
                {
                    if (TryGetNavPoint(transform.position + Random.insideUnitSphere * wanderRadius, wanderRadius, out Vector3 newPos))
                    {
                        if (agent != null) agent.SetDestination(newPos);
                    }
                    timer = 0;
                }
                isRunning = false;
                break;

            case State.Flee:
                if (agent != null) agent.speed = 4.5f;

                // Direction from the player to the animal = away from the player
                Vector3 awayDir = transform.position - player.position;
                awayDir.y = 0f;
                if (awayDir.sqrMagnitude < 0.01f)
                {
                    awayDir = transform.forward;
                }

                Vector3 fleePos = transform.position + awayDir.normalized * fleeStep;

                // Only use the point if it is on the NavMesh
                if (TryGetNavPoint(fleePos, fleeStep, out Vector3 navFleePos))
                {
                    if (agent != null) agent.SetDestination(navFleePos);
                }
                isRunning = true;
                break;
        }

        // Keep the agent's internal position in sync with where the animal really is
        if (agent != null)
        {
            agent.nextPosition = transform.position;
        }

        // Next point along the path
        Vector3 targetPosition = agent != null ? agent.steeringTarget : transform.position;
        Vector3 worldDelta = targetPosition - transform.position;
        worldDelta.y = 0f;

        if (mover != null && worldDelta.magnitude > 0.2f)
        {
            // CreatureMover reads the axis relative to the target direction,
            // so "forward" (0, 1) means "move towards the target".
            mover.SetInput(Vector2.up, targetPosition, isRunning, false);
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

    // Finds the closest point on the NavMesh. Returns false if none was found.
    bool TryGetNavPoint(Vector3 position, float searchRadius, out Vector3 result)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit navHit, searchRadius, NavMesh.AllAreas))
        {
            result = navHit.position;
            return true;
        }
        result = transform.position;
        return false;
    }
}
