using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void Exit()
    {
        // This code only exists when running inside the Unity Editor
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        // This code only exists in the actual built application
        Application.Quit();
        #endif
    }
}
