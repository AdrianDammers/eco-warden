
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "AnimalDatabase",
    menuName = "Eco-Warden/Animal Database"
)]
public class AnimalDatabase : ScriptableObject
{
    public List<AnimalData> animals = new List<AnimalData>();

    private Dictionary<string, AnimalData> animalLookup;

    public void Initialize()
    {
        animalLookup = new Dictionary<string, AnimalData>();

        foreach (AnimalData animal in animals)
        {
            if (animal == null || string.IsNullOrWhiteSpace(animal.animalID))
                continue;

            if (!animalLookup.ContainsKey(animal.animalID))
                animalLookup.Add(animal.animalID, animal);
        }
    }

    public bool TryGetAnimal(string id, out AnimalData animal)
    {
        if (animalLookup == null)
            Initialize();

        return animalLookup.TryGetValue(id, out animal);
    }
}
