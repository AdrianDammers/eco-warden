using UnityEngine;
using UnityEngine.InputSystem;

// Lets the player treat sick animals: look at the animal, stand close and press E.
// Put this on the Main Camera (next to PlayerRifle).
public class PlayerTreat : MonoBehaviour
{
    public float treatDistance = 3f;   // how close the player must be

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        // Ray from the middle of the screen, like the rifle, but short
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, treatDistance))
        {
            Debug.Log("Nothing close enough to treat");
            return;
        }

        Animal animal = hit.collider.GetComponentInParent<Animal>();
        if (animal == null)
        {
            Debug.Log("That's not an animal");
            return;
        }

        if (!animal.Treat())
        {
            Debug.Log(animal.name + " is not sick");
        }
    }
}
