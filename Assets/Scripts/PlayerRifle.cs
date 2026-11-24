using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Simple first person rifle: builds a low-poly rifle from basic shapes,
// shoots a ray from the middle of the screen and kicks back a little.
// Put this on the Main Camera (the child of the player).
public class PlayerRifle : MonoBehaviour
{
    // Other scripts (e.g. animal hearing) can listen to this: called with the shot position
    public static event Action<Vector3> OnShot;

    [Header("Shooting")]
    public float range = 100f;
    public float fireCooldown = 1.0f;   // seconds between shots (bolt-action feel)

    [Header("Rifle placement (relative to the camera)")]
    public Vector3 rifleOffset = new Vector3(0.25f, -0.25f, 0.5f);

    [Header("Recoil")]
    public float recoilDistance = 0.08f;
    public float recoilReturnSpeed = 8f;

    private Transform rifle;
    private Camera cam;
    private float nextFireTime;
    private float currentRecoil;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.nearClipPlane = 0.05f; // so the rifle isn't cut off when it's close to the camera
        rifle = BuildRifle();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireCooldown;
        }

        // Move the rifle back towards its rest position after the recoil
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, recoilReturnSpeed * Time.deltaTime);
        rifle.localPosition = rifleOffset + Vector3.back * currentRecoil;
        rifle.localRotation = Quaternion.Euler(-currentRecoil * 60f, 0f, 0f);
    }

    void Shoot()
    {
        currentRecoil = recoilDistance;
        OnShot?.Invoke(transform.position);

        // Ray from the middle of the screen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Did we hit an animal? (GetComponentInParent also works if we hit a child part of it)
            Animal animal = hit.collider.GetComponentInParent<Animal>();
            if (animal != null)
            {
                animal.Hit();
            }
        }
        else
        {
            Debug.Log("Missed");
        }
    }

    // A small crosshair dot in the middle of the screen
    void OnGUI()
    {
        float size = 6f;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(Screen.width / 2f - size / 2f, Screen.height / 2f - size / 2f, size, size),
                        Texture2D.whiteTexture);
    }

    // Builds the rifle from cubes and cylinders in a low-poly style
    Transform BuildRifle()
    {
        Material wood = MakeMaterial(new Color(0.45f, 0.28f, 0.15f));
        Material metal = MakeMaterial(new Color(0.18f, 0.18f, 0.2f));

        Transform root = new GameObject("Rifle").transform;
        root.SetParent(transform, false);
        root.localPosition = rifleOffset;

        // Stock (back part, against the shoulder)
        AddPart(root, PrimitiveType.Cube, wood, new Vector3(0f, -0.03f, -0.22f), new Vector3(0.05f, 0.09f, 0.3f), Vector3.zero);
        // Body
        AddPart(root, PrimitiveType.Cube, wood, new Vector3(0f, 0f, 0.1f), new Vector3(0.05f, 0.06f, 0.4f), Vector3.zero);
        // Barrel (cylinders point up by default, so rotate them forward)
        AddPart(root, PrimitiveType.Cylinder, metal, new Vector3(0f, 0.03f, 0.45f), new Vector3(0.025f, 0.25f, 0.025f), new Vector3(90f, 0f, 0f));
        // Scope
        AddPart(root, PrimitiveType.Cylinder, metal, new Vector3(0f, 0.07f, 0.08f), new Vector3(0.035f, 0.1f, 0.035f), new Vector3(90f, 0f, 0f));
        // Trigger guard
        AddPart(root, PrimitiveType.Cube, metal, new Vector3(0f, -0.05f, 0.02f), new Vector3(0.02f, 0.03f, 0.05f), Vector3.zero);

        return root;
    }

    void AddPart(Transform parent, PrimitiveType type, Material mat, Vector3 pos, Vector3 scale, Vector3 rotation)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        Destroy(part.GetComponent<Collider>()); // the rifle shouldn't block our own shots
        part.transform.SetParent(parent, false);
        part.transform.localPosition = pos;
        part.transform.localScale = scale;
        part.transform.localEulerAngles = rotation;
        part.GetComponent<Renderer>().sharedMaterial = mat;
    }

    Material MakeMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.1f); // matte, like the rest of the low-poly assets
        return mat;
    }
}
