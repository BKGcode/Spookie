using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IconSet", menuName = "Spookie/Task/Icon Set", order = 0)]
public class IconSetSO : ScriptableObject
{
    public List<Sprite> taskIcons = new List<Sprite>();

    // Runtime state for keeping track of available icons in the current cycle
    private List<int> availableIconIndices;

    public Sprite GetIconByIndex(int index)
    {
        if (taskIcons == null || index < 0 || index >= taskIcons.Count)
        {
            // Return null or a default placeholder sprite if index is invalid
            Debug.LogWarning($"Invalid icon index requested: {index}");
            return null; 
        }
        return taskIcons[index];
    }

    // Gets a random icon index, ensuring all icons are used once before repeating in a cycle.
    public int GetRandomIconIndex()
    {
        // Safety check: No icons defined
        if (taskIcons == null || taskIcons.Count == 0)
        {
            Debug.LogWarning("IconSetSO: No task icons defined in the list.");
            return 0; // Return default index
        }

        // Initialize or reset the available list if it's empty (start of a new cycle)
        if (availableIconIndices == null || availableIconIndices.Count == 0)
        {
            availableIconIndices = new List<int>();
            for (int i = 0; i < taskIcons.Count; i++)
            {
                availableIconIndices.Add(i);
            }
            // Optional: Shuffle the list once at the start of the cycle for potentially better distribution?
            // Shuffle(availableIconIndices); // If you implement a Shuffle utility
            Debug.Log("IconSetSO: Starting new icon selection cycle.");
        }

        // Pick a random index FROM THE AVAILABLE LIST
        int randomIndexInAvailableList = Random.Range(0, availableIconIndices.Count);

        // Get the actual icon index stored at that position
        int chosenIconIndex = availableIconIndices[randomIndexInAvailableList];

        // Remove the chosen index from the available list for this cycle
        availableIconIndices.RemoveAt(randomIndexInAvailableList);

        return chosenIconIndex;
    }

    // Optional: Utility to shuffle a list (Fisher-Yates algorithm)
    /*
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    */
}

// --- Summary Block ---
// ScriptRole: Holds a configurable list of Sprites to be used as icons for tasks. Provides safe access to icons by index.
// RelatedScripts: TaskManager (references this to get icons), TaskData (stores icon index)
// UsesSO: None 