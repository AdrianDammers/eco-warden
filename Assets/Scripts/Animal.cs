using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using ithappy.Animals_FREE;

// Sits on every animal. It tells which species the animal is,
// and handles what happens when the animal is shot.
public class Animal : MonoBehaviour
{
    // Other scripts (e.g. ScoreManager) listen to this: called with the species that was shot
    public static event Action<AnimalData> OnAnimalShot;
    // Called with the species when a sick animal is treated by the player
    public static event Action<AnimalData> OnAnimalTreated;

    [Tooltip("Drag the species asset here, e.g. Deer or Horse from Scripts/AnimalGuide")]
    public AnimalData species;

    [Header("Death")]
    public float fallDuration = 0.6f;  // how long it takes to tip over
    public float lieDownHeight = 0.2f; // lifts the body a bit so it doesn't sink into the ground

    public bool IsDead { get; private set; }

    // Called by the rifle when a shot hits this animal
    public void Hit()
    {
        if (IsDead) return; // only count each animal once
        IsDead = true;

        if (species == null)
            Debug.LogWarning(name + " has no species assigned!");

        OnAnimalShot?.Invoke(species);
        Die();
    }

    // Called by PlayerTreat. Returns true if the animal was sick and is now cured.
    public bool Treat()
    {
        if (IsDead) return false;

        AnimalFSM fsm = GetComponent<AnimalFSM>();
        if (fsm == null || !fsm.IsSick) return false;

        fsm.Cure();
        OnAnimalTreated?.Invoke(species);
        return true;
    }

    void Die()
    {
        // Stop all behaviour: no more thinking, sensing, moving or animating
        DisableIfFound<AnimalFSM>();
        DisableIfFound<AnimalPerception>();
        DisableIfFound<CreatureMover>();
        DisableIfFound<NavMeshAgent>();

        // The body collider is not needed anymore (it's a Collider, not a Behaviour, so it's handled here)
        CharacterController body = GetComponent<CharacterController>();
        if (body != null) body.enabled = false;

        // Freeze the animation in its current pose
        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = false;

        StartCoroutine(FallOver());
    }

    // Tips the animal over onto its side, then it stays there
    IEnumerator FallOver()
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, 90f); // roll onto the side
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * lieDownHeight;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fallDuration;
            float eased = t * t; // starts slow, falls faster at the end
            transform.rotation = Quaternion.Slerp(startRot, endRot, eased);
            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }
    }

    void DisableIfFound<T>() where T : Behaviour
    {
        T component = GetComponent<T>();
        if (component != null) component.enabled = false;
    }
}
