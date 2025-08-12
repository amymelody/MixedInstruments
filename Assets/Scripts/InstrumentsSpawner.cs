using UnityEngine;

public class InstrumentsSpawner : MonoBehaviour
{
    [SerializeField]
    Keyboard m_KeyboardPrefab;

    [SerializeField]
    float m_KeyboardFrontPadding = 0.06f;

    bool m_Spawned;
    Keyboard m_Keyboard;

    public void Spawn(float tableWidth, float tableDepth)
    {
        if (m_Spawned)
            return;

        m_Keyboard = Instantiate(m_KeyboardPrefab, transform);
        var keyboardRelativeBounds = m_Keyboard.GetRelativeBounds();
        m_Keyboard.transform.localPosition = new Vector3(
            -keyboardRelativeBounds.center.x,
            0f,
            (-tableDepth * 0.5f) + m_KeyboardFrontPadding - keyboardRelativeBounds.center.z + keyboardRelativeBounds.extents.z);

        m_Spawned = true;
    }
    
    public void Despawn()
    {
        if (!m_Spawned)
            return;

        m_Spawned = false;
        Destroy(m_Keyboard.gameObject);
    }
}
