using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class MapViewController : MonoBehaviour
{
    public GameObject buttonPanel;
    public GameObject mapPanel;
    public GameObject fullscreenPanel;
    public GameObject editMapButton;

    public void OpenFullscreenPanel()
    {
        buttonPanel.SetActive(false);
        mapPanel.SetActive(false);
        fullscreenPanel.SetActive(true);
    }

    public void CloseFullscreenPanel()
    {
        buttonPanel.SetActive(true);
        mapPanel.SetActive(true);
        fullscreenPanel.SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && fullscreenPanel.activeSelf)
        {
            CloseFullscreenPanel();
        }
    }

    public void Start()
    {
        string folder = PlayerPrefs.GetString("LastMapFolder", "");
        string imagePath = FindMapImage(folder);
        string name = System.IO.Path.GetFileNameWithoutExtension(imagePath);
        if(name == "colorMask")
        {
            editMapButton.SetActive(true);
        }
        else
        {
            editMapButton.SetActive(false);
        }
    }

    string FindMapImage(string folder)
    {
        string[] extensions = { "*.png", "*.jpg", "*.jpeg" };

        foreach (string ext in extensions)
        {
            string[] files = Directory.GetFiles(folder, ext);
            if (files.Length > 0)
            {
                return files[0]; // return first match
            }
        }

        return null;
    }
}
