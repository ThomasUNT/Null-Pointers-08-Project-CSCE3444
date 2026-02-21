using UnityEngine;

public class MapClickHandler : MonoBehaviour
{
    public Camera mapCamera;
    public MapDataManager dataManager;

    public bool placeMode = false;

    void Update()
    {
        if (!placeMode) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = mapCamera.ScreenToWorldPoint(mousePos);

            Vector2 mapPos = new Vector2(worldPos.x, worldPos.y);

            dataManager.AddNode(mapPos);

            Debug.Log("Node placed at: " + mapPos);
        }
    }
}