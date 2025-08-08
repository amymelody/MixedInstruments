using UnityEngine;

public class InstrumentsSpawner : MonoBehaviour
{
    [SerializeField]
    Keyboard m_KeyboardPrefab;

    [SerializeField]
    float m_KeyboardFrontPadding = 0.06f;

    bool m_Spawned;

    public void Spawn(float tableWidth, float tableDepth)
    {
        if (m_Spawned)
            return;

        var keyboard = Instantiate(m_KeyboardPrefab, transform);
        var keyboardRelativeBounds = keyboard.GetRelativeBounds();
        keyboard.transform.localPosition = new Vector3(
            -keyboardRelativeBounds.center.x,
            0f,
            (-tableDepth * 0.5f) + m_KeyboardFrontPadding - keyboardRelativeBounds.center.z + keyboardRelativeBounds.extents.z);

        m_Spawned = true;
    }
}
