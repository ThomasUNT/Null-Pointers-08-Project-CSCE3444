using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MapFullscreenHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject leftPanel;
    public GameObject exitFSButton;
    public RectTransform mapWindow;
    public MapController mapController;

    public bool isFullscreen = false;

    // We store the original values to "snap" back to the sidebar layout
    private Vector2 originalAnchorMin;
    private Vector2 originalOffsetMin;

    void Start()
    {
        if (mapWindow != null)
        {
            // Store the initial setup (where the left edge starts after the side panel)
            originalAnchorMin = mapWindow.anchorMin;
            originalOffsetMin = mapWindow.offsetMin;
        }
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            // Don't toggle fullscreen if we're typing in an input field
            if (EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null ||
                EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null)
            {
                return;
            }
        }

        // Toggle with F key
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFullscreen();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isFullscreen)
        {
            ToggleFullscreen();
        }
    }

    public void ToggleFullscreen()
    {
        isFullscreen = !isFullscreen;

        // Hide/Show the UI panel
        if (leftPanel != null)
            leftPanel.SetActive(!isFullscreen);

        // Adjust Map Window to fill the space
        if (isFullscreen)
        {
            // Stretch to the absolute left of the screen
            mapWindow.anchorMin = new Vector2(0, 0);
            mapWindow.offsetMin = new Vector2(0, 0);
            exitFSButton.SetActive(true);
        }
        else
        {
            // Return to original sidebar position
            mapWindow.anchorMin = originalAnchorMin;
            mapWindow.offsetMin = originalOffsetMin;
            exitFSButton.SetActive(false);
        }

        if (mapController != null)
        {
            // We invoke it slightly delayed or next frame to let UI rebuild
            StartCoroutine(RefreshMapLayout());
        }
    }

    private System.Collections.IEnumerator RefreshMapLayout()
    {
        yield return new WaitForEndOfFrame();
        mapController.ClampToWindow();
    }
}