using UnityEngine;
using TMPro;

public class NodeEditorUI : MonoBehaviour
{
    public GameObject nodeTextInputPanel;
    public GameObject buttonPanel;
    public TMP_InputField inputField;

    private int activeNodeIndex = -1;

    public void OpenEditor(int nodeIndex)
    {
        activeNodeIndex = nodeIndex;

        Debug.Log("OpenEditor called for node " + nodeIndex);

        nodeTextInputPanel.SetActive(true);
        buttonPanel.SetActive(false);

        inputField.text = "";

        // focus cursor automatically
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void CloseEditor()
    {
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(true);
        activeNodeIndex = -1;
    }

    public string GetText()
    {
        return inputField.text;
    }
}
