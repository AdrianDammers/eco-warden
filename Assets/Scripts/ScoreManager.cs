using System;
using UnityEngine;

// Keeps track of the score. Listens to Animal.OnAnimalShot and gives
// points or a penalty depending on the species' conservation status.
// Put this on one empty GameObject in the scene.
public class ScoreManager : MonoBehaviour
{
    // The HUD can listen to this to show the new score
    public static event Action<int> OnScoreChanged;

    [Header("Points per status")]
    public int invasivePoints = 10;     // allowed to shoot
    public int protectedPenalty = -20;  // endangered, worst mistake
    public int nativePenalty = -10;     // native wildlife, not allowed
    public int domesticPenalty = -10;   // farm animals, not allowed

    public int Score { get; private set; }

    void OnEnable()  { Animal.OnAnimalShot += HandleAnimalShot; }
    void OnDisable() { Animal.OnAnimalShot -= HandleAnimalShot; }

    void HandleAnimalShot(AnimalData species)
    {
        if (species == null) return;

        int change = PointsFor(species.status);
        Score += change;

        Debug.Log($"Shot {species.speciesName} ({species.status}): {change:+#;-#;0}. Score: {Score}");
        OnScoreChanged?.Invoke(Score);
    }

    int PointsFor(AnimalStatus status)
    {
        switch (status)
        {
            case AnimalStatus.Invasive:  return invasivePoints;
            case AnimalStatus.Protected: return protectedPenalty;
            case AnimalStatus.Native:    return nativePenalty;
            case AnimalStatus.Domestic:  return domesticPenalty;
            default:                     return 0;
        }
    }
}
