using Melanchall.DryWetMidi.MusicTheory;
using System;
using Unity.XR.CoreUtils;
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

    [SerializeField]
    Transform m_VisualCube;

    public float width { get; private set; }

    public float depth { get; private set; }

    public float height => m_Height;

    XROrigin m_XROrigin;
    XRHandTrackingEvents m_LeftHandEvents;
    XRHandTrackingEvents m_RightHandEvents;

    Vector2 m_FrequencyRange;

    Material m_CubeMaterial;

    public void SetDimensions(float width, float depth)
    {
        this.width = width;
        this.depth = depth;
        m_VisualCube.transform.localPosition = Vector3.up * height * 0.5f;
        m_VisualCube.transform.localScale = new Vector3(width, height, depth) + Vector3.one * (m_Padding * 2f);
    }

    void OnEnable()
    {
        m_FrequencyRange = new Vector2(
            NoteUtils.FundamentalFrequencies[NoteUtilities.GetNoteNumber(m_LowestNote.NoteName, m_LowestNote.Octave)],
            NoteUtils.FundamentalFrequencies[NoteUtilities.GetNoteNumber(m_HighestNote.NoteName, m_HighestNote.Octave)]
            );

        Debug.Log("frequency range: " + m_FrequencyRange);

        m_XROrigin = FindAnyObjectByType<XROrigin>();
        foreach (var handEvents in FindObjectsByType<XRHandTrackingEvents>(FindObjectsSortMode.None))
        {
            if (m_LeftHandEvents == null && handEvents.handedness == Handedness.Left)
                m_LeftHandEvents = handEvents;
            else if (m_RightHandEvents == null && handEvents.handedness == Handedness.Right)
                m_RightHandEvents = handEvents;
        }

        m_LeftHandEvents.jointsUpdated.AddListener(OnJointsUpdated);
        m_RightHandEvents.jointsUpdated.AddListener(OnJointsUpdated);

        m_CubeMaterial = m_VisualCube.GetComponent<Renderer>().material;
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

        var cubeRed = m_CubeMaterial.color.r;
        var cubeGreen = m_CubeMaterial.color.g;
        var cubeBlue = m_CubeMaterial.color.b;
        var cubeAlpha = m_CubeMaterial.color.a;

        var indexTip = eventArgs.hand.GetJoint(XRHandJointID.IndexTip);
        if (!indexTip.TryGetPose(out var indexTipPose) || !IsPointInVolume(m_XROrigin.transform.TransformPoint(indexTipPose.position), out var pointNormPos))
        {
            amp.volume = 0f;

            switch (eventArgs.hand.handedness)
            {
                case Handedness.Left:
                    m_CubeMaterial.color = new Color(0f, cubeGreen, cubeBlue, cubeAlpha);
                    break;
                case Handedness.Right:
                    m_CubeMaterial.color = new Color(cubeRed, cubeGreen, 0f, cubeAlpha);
                    break;
            }

            return;
        }

        var indexShape = eventArgs.hand.CalculateFingerShape(XRHandFingerID.Index, XRFingerShapeTypes.Pinch);
        if (!indexShape.TryGetPinch(out var pinch))
            return;

        // TODO: should be exponential
        // TODO: fix crackling
        amp.frequency = Mathf.Lerp(m_FrequencyRange.x, m_FrequencyRange.y, pointNormPos.y);
        amp.volume = pinch;

        switch (eventArgs.hand.handedness)
        {
            case Handedness.Left:
                m_CubeMaterial.color = new Color(pinch, cubeGreen, cubeBlue, cubeAlpha);
                break;
            case Handedness.Right:
                m_CubeMaterial.color = new Color(cubeRed, cubeGreen, pinch, cubeAlpha);
                break;
        }
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
