using Melanchall.DryWetMidi.MusicTheory;
using System;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;

[Serializable]
public struct SerializedNote
{
    public NoteName NoteName;
    public int Octave;
}

public class PlayVolume : MonoBehaviour
{
    [SerializeField]
    float m_Height = 1.5f;

    [SerializeField]
    float m_Padding = 0.1f;

    [SerializeField]
    FreeOscillatorAmp m_LeftAmp;

    [SerializeField]
    FreeOscillatorAmp m_RightAmp;

    [SerializeField]
    SerializedNote m_LowestNote = new() { NoteName = NoteName.C, Octave = 0 };

    [SerializeField]
    SerializedNote m_HighestNote = new() { NoteName = NoteName.C, Octave = 6 };

    public float width { get; set; }

    public float depth { get; set; }

    public float height
    {
        get => m_Height;
        set => m_Height = value;
    }

    XRHandTrackingEvents m_LeftHandEvents;
    XRHandTrackingEvents m_RightHandEvents;

    Vector2 m_FrequencyRange;

    void OnEnable()
    {
        m_FrequencyRange = new Vector2(
            NoteUtils.FundementalFrequencies[NoteUtilities.GetNoteNumber(m_LowestNote.NoteName, m_LowestNote.Octave)],
            NoteUtils.FundementalFrequencies[NoteUtilities.GetNoteNumber(m_HighestNote.NoteName, m_HighestNote.Octave)]
            );

        foreach (var handEvents in FindObjectsByType<XRHandTrackingEvents>(FindObjectsSortMode.None))
        {
            if (m_LeftHandEvents == null && handEvents.handedness == Handedness.Left)
                m_LeftHandEvents = handEvents;
            else if (m_RightHandEvents == null && handEvents.handedness == Handedness.Right)
                m_RightHandEvents = handEvents;
        }

        m_LeftHandEvents.jointsUpdated.AddListener(OnJointsUpdated);
        m_RightHandEvents.jointsUpdated.AddListener(OnJointsUpdated);
    }

    void OnDisable()
    {
        m_LeftHandEvents.jointsUpdated.RemoveListener(OnJointsUpdated);
        m_RightHandEvents.jointsUpdated.RemoveListener(OnJointsUpdated);
    }

    void OnJointsUpdated(XRHandJointsUpdatedEventArgs eventArgs)
    {
        if (!isActiveAndEnabled || !eventArgs.hand.isTracked)
            return;

        FreeOscillatorAmp amp = null;
        switch (eventArgs.hand.handedness)
        {
            case Handedness.Left:
                amp = m_LeftAmp;
                break;
            case Handedness.Right:
                amp = m_RightAmp;
                break;
        }

        if (amp == null)
            return;

        var indexTip = eventArgs.hand.GetJoint(XRHandJointID.IndexTip);
        if (!indexTip.TryGetPose(out var indexTipPose) || !IsPointInVolume(indexTipPose.position, out var pointNormPos))
        {
            amp.volume = 0f;
            return;
        }

        var indexShape = eventArgs.hand.CalculateFingerShape(XRHandFingerID.Index, XRFingerShapeTypes.Pinch);
        if (!indexShape.TryGetPinch(out var pinch))
            return;

        amp.frequency = Mathf.Lerp(m_FrequencyRange.x, m_FrequencyRange.y, pointNormPos.y);
        amp.volume = pinch;
    }

    bool IsPointInVolume(Vector3 point, out Vector3 normalizedPosition)
    {
        var halfWidth = width * 0.5f;
        var halfDepth = depth * 0.5f;
        var bottomPos = transform.position;
        normalizedPosition = new Vector3(
            Mathf.InverseLerp(bottomPos.x - halfWidth, bottomPos.x + halfWidth, point.x),
            Mathf.InverseLerp(bottomPos.y, bottomPos.y + height, point.y),
            Mathf.InverseLerp(bottomPos.z - halfDepth, bottomPos.z + halfDepth, point.z)
            );

        return point.y >= bottomPos.y - m_Padding && point.y <= bottomPos.y + height + m_Padding &&
            point.x >= bottomPos.x - halfWidth - m_Padding && point.x <= bottomPos.x + halfWidth + m_Padding &&
            point.z >= bottomPos.z - halfDepth - m_Padding && point.z <= bottomPos.z + halfDepth + m_Padding;
    }
}
