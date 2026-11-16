
using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleUI : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private MonoBehaviour[] playerScriptsToDisable;

    private PlayerRifle playerRifle;
    private bool isOpen = false;

    void Awake()
    {
        playerRifle = FindAnyObjectByType<PlayerRifle>();
        if (playerRifle == null)
            Debug.LogWarning("PlayerRifle not found!");
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.gKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    void Toggle()
    {
        isOpen = !isOpen;

        targetObject.SetActive(isOpen);

        foreach (MonoBehaviour script in playerScriptsToDisable)
        {
            if (script != null)
                script.enabled = !isOpen;
        }

        // Disable shooting
        if (playerRifle != null)
            playerRifle.enabled = !isOpen;

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}
