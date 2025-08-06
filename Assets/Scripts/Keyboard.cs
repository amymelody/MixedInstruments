using UnityEngine;

public class Keyboard : MonoBehaviour
{
    const string k_KeyObjNameFormat = "Key_{0}{1}";

    [SerializeField]
    Transform m_KeysRoot;

    [SerializeField]
    GameObject m_KeyWhiteLPrefab;

    [SerializeField]
    GameObject m_KeyWhiteMPrefab;

    [SerializeField]
    GameObject m_KeyWhiteRPrefab;

    [SerializeField]
    GameObject m_KeyBlackPrefab;

    [SerializeField]
    float m_HalfOffset = 0.017f;

    void Start()
    {
        var next = SpawnOctave(4, 0f);
        next = SpawnOctave(5, next);
        SpawnKey(NoteName.C, 6, next);
    }

    float SpawnOctave(int octave, float xPos)
    {
        var next = SpawnKey(NoteName.C, octave, xPos);
        next = SpawnKey(NoteName.Csharp, octave, next);
        next = SpawnKey(NoteName.D, octave, next);
        next = SpawnKey(NoteName.Dsharp, octave, next);
        next = SpawnKey(NoteName.E, octave, next);
        next = SpawnKey(NoteName.F, octave, next);
        next = SpawnKey(NoteName.Fsharp, octave, next);
        next = SpawnKey(NoteName.G, octave, next);
        next = SpawnKey(NoteName.Gsharp, octave, next);
        next = SpawnKey(NoteName.A, octave, next);
        next = SpawnKey(NoteName.Asharp, octave, next);
        next = SpawnKey(NoteName.B, octave, next);
        return next;
    }

    float SpawnKey(NoteName noteName, int octave, float xPos)
    {
        GameObject prefab;
        float nextOffset;
        switch (noteName)
        {
            case NoteName.C:
            case NoteName.F:
                prefab = m_KeyWhiteLPrefab;
                nextOffset = m_HalfOffset;
                break;
            case NoteName.D:
            case NoteName.G:
            case NoteName.A:
                prefab = m_KeyWhiteMPrefab;
                nextOffset = m_HalfOffset;
                break;
            case NoteName.E:
            case NoteName.B:
                prefab = m_KeyWhiteRPrefab;
                nextOffset = m_HalfOffset * 2f;
                break;
            default:
                prefab = m_KeyBlackPrefab;
                nextOffset = m_HalfOffset;
                break;
        }

        var key = Instantiate(prefab, m_KeysRoot);
        key.name = string.Format(k_KeyObjNameFormat, noteName, octave);
        key.transform.localPosition = Vector3.right * xPos;
        return xPos + nextOffset;
    }
}
