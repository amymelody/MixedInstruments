using UnityEngine;

public class InstrumentsSpawner : MonoBehaviour
{
    [SerializeField]
    Keyboard m_KeyboardPrefab;

    [SerializeField]
    float m_KeyboardFrontPadding = 0.06f;

    [SerializeField]
    PlayVolume m_PlayVolumePrefab;

    bool m_Spawned;
    Keyboard m_Keyboard;
    PlayVolume m_PlayVolume;

    public void Spawn(float tableWidth, float tableDepth)
    {
        Debug.Log("TRY SPAWN");
        if (m_Spawned)
            return;

        SpawnPlayVolume(tableWidth, tableDepth);
        Debug.Log("SPAWNED");
        m_Spawned = true;
    }
    
    public void Despawn()
    {
        Debug.Log("TRY DESPAWN");
        if (!m_Spawned)
            return;

        m_Spawned = false;
        DespawnPlayVolume();
        Debug.Log("DESPAWNED");
    }

    void SpawnKeyboard(float tableWidth, float tableDepth)
    {
        m_Keyboard = Instantiate(m_KeyboardPrefab, transform);
        var keyboardRelativeBounds = m_Keyboard.GetRelativeBounds();
        m_Keyboard.transform.localPosition = new Vector3(
            -keyboardRelativeBounds.center.x,
            0f,
            (-tableDepth * 0.5f) + m_KeyboardFrontPadding - keyboardRelativeBounds.center.z + keyboardRelativeBounds.extents.z);
    }

    void DespawnKeyboard()
    {
        Destroy(m_Keyboard.gameObject);
    }

    void SpawnPlayVolume(float tableWidth, float tableDepth)
    {
        m_PlayVolume = Instantiate(m_PlayVolumePrefab, transform);
        m_PlayVolume.SetDimensions(tableWidth, tableDepth);
    }

    void DespawnPlayVolume()
    {
        Destroy(m_PlayVolume.gameObject);
    }
}
