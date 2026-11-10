
using UnityEngine;

public enum AnimalStatus
{
    Protected,
    Invasive,
    Native,
    Domestic
}

[CreateAssetMenu(
    fileName = "NewAnimal",
    menuName = "Eco-Warden/Animal Data"
)]
public class AnimalData : ScriptableObject
{
    [Header("Identification")]
    public string animalID;
    public string speciesName;
    public string scientificName;
    public Sprite image;

    [Header("Conservation")]
    public AnimalStatus status;

    [TextArea(3, 6)]
    public string description;

    [TextArea(2, 4)]
    public string identificationFeatures;

    public string habitat;
    public string behaviour;

    [TextArea(2, 4)]
    public string gameplayHint;
}
