using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class CurvedText : MonoBehaviour
{
    TMP_Text tmp;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
    }

    // arcValue range: 0 (flat) to 1 (strong upward arc)
    public void UpdateCurve(float arcValue)
    {
        tmp.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmp.textInfo;
        int charCount = textInfo.characterCount;

        if (charCount == 0)
            return;

        // If flat, reset mesh and exit
        if (arcValue <= 0.001f)
        {
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            return;
        }

        // How strong the arc can get
        float maxArcDegrees = 120f;

        float totalArc = arcValue * maxArcDegrees;
        float anglePerChar = totalArc / Mathf.Max(1, charCount - 1);
        float startAngle = -totalArc / 2f;

        // Radius derived from arc size to prevent squishing
        float radius = (textInfo.characterInfo[charCount - 1].origin) / totalArc * Mathf.Rad2Deg;
        radius = Mathf.Max(radius, 50f); // minimum radius safeguard

        for (int i = 0; i < charCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Character midpoint
            Vector3 charMid = (vertices[vertexIndex] +
                               vertices[vertexIndex + 2]) / 2;

            float angle = startAngle + anglePerChar * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Sin(rad) * radius,
                Mathf.Cos(rad) * radius - radius,
                0
            );

            Matrix4x4 matrix = Matrix4x4.TRS(
                offset,
                Quaternion.Euler(0, 0, (-angle / 2)),
                Vector3.one
            );

            for (int j = 0; j < 4; j++)
            {
                Vector3 orig = vertices[vertexIndex + j] - charMid;
                vertices[vertexIndex + j] =
                    matrix.MultiplyPoint3x4(orig) + charMid;
            }
        }

        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}