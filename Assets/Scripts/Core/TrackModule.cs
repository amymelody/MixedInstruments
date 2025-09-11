using UnityEngine;

public class TrackModule : Module
{
    [SerializeField]
    MidiInstrument m_Instrument;

    [SerializeField]
    TrackClipUI m_ClipUI;

    [SerializeField]
    float m_FrontUIPadding = 0.05f;

    public MidiInstrument instrument => m_Instrument;

    public override void AttachToTabletop(Tabletop tabletop)
    {
        m_ClipUI.transform.localPosition = new Vector3(0f, 0f, -tabletop.depth * 0.5f + m_FrontUIPadding);
    }

    void OnEnable()
    {
        m_ClipUI.AssignTrack(this);
    }
}
