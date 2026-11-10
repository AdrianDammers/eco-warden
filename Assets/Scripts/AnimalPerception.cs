using UnityEngine;

// SENSE: what the animal can see and hear.
// The FSM asks this script if the animal has noticed the player.
public class AnimalPerception : MonoBehaviour
{
    [Header("Vision")]
    public float viewDistance = 10f;  // how far the animal can see
    public float viewAngle = 120f;    // width of the vision cone in degrees
    public float eyeHeight = 0.8f;    // where the "eyes" are, above the animal's feet

    [Header("Hearing")]
    public float hearingRadius = 25f; // gunshots closer than this are heard

    private Transform player;
    private bool heardShot;

    // Start listening to gunshots when the animal is active, stop when it is removed
    void OnEnable()  { PlayerRifle.OnShot += HandleShot; }
    void OnDisable() { PlayerRifle.OnShot -= HandleShot; }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // ---------- VISION ----------
    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 eyes = transform.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = player.position - eyes;

        // 1. Is the player close enough?
        if (toPlayer.magnitude > viewDistance) return false;

        // 2. Is the player inside the vision cone (in front of the animal)?
        if (Vector3.Angle(transform.forward, toPlayer) > viewAngle / 2f) return false;

        // 3. Is anything blocking the view (trees, rocks)?
        if (Physics.Raycast(eyes, toPlayer.normalized, out RaycastHit hit, viewDistance))
        {
            return hit.transform == player;
        }
        return false;
    }

    // ---------- HEARING ----------
    // Called by PlayerRifle every time the player shoots
    void HandleShot(Vector3 shotPosition)
    {
        if (Vector3.Distance(transform.position, shotPosition) <= hearingRadius)
        {
            heardShot = true;
        }
    }

    // Returns true once after a shot was heard, then resets
    public bool HeardShot()
    {
        bool result = heardShot;
        heardShot = false;
        return result;
    }

    // Draws the vision cone and hearing radius in the Scene view when the animal is selected
    void OnDrawGizmosSelected()
    {
        Vector3 eyes = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(eyes, eyes + left * viewDistance);
        Gizmos.DrawLine(eyes, eyes + right * viewDistance);
        Gizmos.DrawWireSphere(eyes, viewDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}
