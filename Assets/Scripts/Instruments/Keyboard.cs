using Melanchall.DryWetMidi.MusicTheory;
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

    MIDIInstrument m_Instrument;

    public Bounds GetRelativeBounds()
    {
        var octaveOffset = m_HalfOffset * 2f * 7f;
        var whiteKeyBounds = GetWhiteKeyBounds();
        var width = whiteKeyBounds.size.x + octaveOffset * 2f;
        var centerX = octaveOffset;
        var center = m_KeysRoot.localPosition + new Vector3(centerX, whiteKeyBounds.center.y, whiteKeyBounds.center.z);
        return new Bounds(center, new Vector3(width, whiteKeyBounds.size.y, whiteKeyBounds.size.z));
    }

    Bounds GetWhiteKeyBounds()
    {
        var bounds = new Bounds();
        foreach (var renderer in m_KeyWhiteLPrefab.GetComponentsInChildren<Renderer>())
        {
            var rendBounds = renderer.bounds;
            rendBounds.center -= m_KeyWhiteLPrefab.transform.position;
            bounds.Encapsulate(rendBounds);
        }

        return bounds;
    }

    void Start()
    {
        m_Instrument = GetComponentInChildren<MIDIInstrument>();

        var next = SpawnOctave(4, 0f);
        next = SpawnOctave(5, next);
        SpawnKey(NoteName.C, 6, next);
    }

    float SpawnOctave(int octave, float xPos)
    {
        var next = SpawnKey(NoteName.C, octave, xPos);
        next = SpawnKey(NoteName.CSharp, octave, next);
        next = SpawnKey(NoteName.D, octave, next);
        next = SpawnKey(NoteName.DSharp, octave, next);
        next = SpawnKey(NoteName.E, octave, next);
        next = SpawnKey(NoteName.F, octave, next);
        next = SpawnKey(NoteName.FSharp, octave, next);
        next = SpawnKey(NoteName.G, octave, next);
        next = SpawnKey(NoteName.GSharp, octave, next);
        next = SpawnKey(NoteName.A, octave, next);
        next = SpawnKey(NoteName.ASharp, octave, next);
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

        var touchNote = key.GetComponent<TouchNote>();
        touchNote.Note = Note.Get(noteName, octave);
        touchNote.onNoteTouchOn += m_Instrument.NoteOn;
        touchNote.onNoteTouchOff += m_Instrument.NoteOff;

        return xPos + nextOffset;
    }
}
