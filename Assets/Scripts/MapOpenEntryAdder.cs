using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapOpenEntryAdder : MonoBehaviour
{
    public GameObject saveEntryContainer;
    public GameObject saveEntryPrefab;
    public MapSavePreviewManager PreviewManager;

    private List<GameObject> saveEntries = new List<GameObject>();

    void OnEnable()
    {
        // Clear old entries
        for (int i = saveEntries.Count-1; i >= 0; i--)
        {
            GameObject.Destroy(saveEntries[i]);
        }
        saveEntries.Clear();


        string mapsRoot = Path.Combine(Application.persistentDataPath, "maps");
        if (!Directory.Exists(mapsRoot))
            Directory.CreateDirectory(mapsRoot);

        string[] mapFolders = Directory.GetDirectories(mapsRoot);

        // TODO: Filter out folders in this directory which are not valid maps


        float entryTopPadding = 20;
        float entryHeight = 130;
        float entryPlusPaddingHeight = entryTopPadding + entryHeight;


        // Set container to appropriate size
        Transform containerTransform = saveEntryContainer.transform;
        RectTransform containerRectTransform = containerTransform.GetComponent<RectTransform>();
        containerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, entryTopPadding + entryPlusPaddingHeight * mapFolders.Length);


        // Add save entries
        for (int entryIdx = 0; entryIdx < mapFolders.Length; entryIdx++)
        {
            string targetFolder = mapFolders[entryIdx];
            GameObject newEntry = Instantiate(saveEntryPrefab, containerTransform);
            RectTransform entryRectTransform = newEntry.transform.GetComponent<RectTransform>();
            entryRectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, entryTopPadding + entryPlusPaddingHeight * entryIdx, entryHeight);

            OpenSave entryOpenSave = newEntry.GetComponent<OpenSave>();
            entryOpenSave.SetPath(targetFolder);
            entryOpenSave.PreviewManager = PreviewManager;

            saveEntries.Add(newEntry);
        }
    }
}
