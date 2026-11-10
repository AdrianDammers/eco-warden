
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimalGuideUI : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private AnimalDatabase database;

    [Header("Panels")]
    [SerializeField] private GameObject animalGrid;
    [SerializeField] private GameObject detailsPanel;

    [Header("Detail Information")]
    [SerializeField] private Image detailImage;
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text scientificName;
    [SerializeField] private TMP_Text detailDescription;
    [SerializeField] private TMP_Text detailHabitat;
    [SerializeField] private TMP_Text detailBehaviour;

    [Header("Animal Buttons")]
    [SerializeField] private Button mooseButton;
    [SerializeField] private Button chickenButton;
    [SerializeField] private Button horseButton;
    [SerializeField] private Button deerButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (database == null)
        {
            Debug.LogError("AnimalDatabase is not assigned!");
            return;
        }

        database.Initialize();

        // Initial state
        animalGrid.SetActive(true);
        detailsPanel.SetActive(false);

        // Animal buttons
        mooseButton.onClick.AddListener(() => ShowAnimal("moose"));
        chickenButton.onClick.AddListener(() => ShowAnimal("chicken"));
        horseButton.onClick.AddListener(() => ShowAnimal("horse"));
        deerButton.onClick.AddListener(() => ShowAnimal("deer"));

        // Close button
        closeButton.onClick.AddListener(CloseDetails);
    }

    public void ShowAnimal(string animalID)
    {
        if (database == null)
            return;

        if (database.TryGetAnimal(animalID, out AnimalData animal))
        {
            // Display image
            detailImage.sprite = animal.image;
            detailImage.preserveAspect = true;

            // Display information
            detailName.text = animal.speciesName;
            scientificName.text = animal.scientificName;
            detailDescription.text = animal.description;
            detailHabitat.text = "Habitat: " + animal.habitat;
            detailBehaviour.text = "Behaviour: " + animal.behaviour;

            // Switch panels
            animalGrid.SetActive(false);
            detailsPanel.SetActive(true);

            Debug.Log("Opened animal guide: " + animal.speciesName);
        }
        else
        {
            Debug.LogWarning("Animal not found: " + animalID);
        }
    }

    public void CloseDetails()
    {
        detailsPanel.SetActive(false);
        animalGrid.SetActive(true);

        Debug.Log("Closed animal details");
    }
}
