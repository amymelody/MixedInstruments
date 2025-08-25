using Melanchall.DryWetMidi.Standards;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[Serializable]
public class DrumSample
{
    public GeneralMidi2ElectronicPercussion midiNote;
    public AudioClip sample;
    public string displayName;
}

public class DrumMachine : Instrument
{
    [SerializeField]
    MidiSampleAmp m_Amp;

    [SerializeField]
    List<DrumSample> m_Samples;

    [SerializeField]
    SampleButton m_SampleButtonPrefab;

    [SerializeField]
    float m_ButtonSize = 0.05f;

    [SerializeField]
    float m_ButtonSpacing = 0.01f;

    [SerializeField]
    int m_ButtonRows = 2;

    [SerializeField]
    int m_ButtonColumns = 8;

    Dictionary<IXRInteractable, SampleButton> m_InteractableToButton = new Dictionary<IXRInteractable, SampleButton>();

    void Start()
    {
        SetUpAmp();
        SpawnButtons();
    }

    void SetUpAmp()
    {
        if (m_Amp == null) m_Amp = GetComponentInChildren<MidiSampleAmp>();
        foreach (var sample in m_Samples)
        {
            m_Amp.AssignSample(sample.midiNote.AsSevenBitNumber(), sample.sample);
        }
    }

    void SpawnButtons()
    {
        m_InteractableToButton.Clear();
        if (m_ButtonRows <= 0) m_ButtonRows = 1;
        if (m_ButtonColumns <= 0) m_ButtonColumns = 1;

        var sizeHalf = m_ButtonSize * 0.5f;
        var rowStart = (m_ButtonSize * m_ButtonRows + m_ButtonSpacing * (m_ButtonRows - 1)) * -0.5f + sizeHalf;
        var columnStart = (m_ButtonSize * m_ButtonColumns + m_ButtonSpacing * (m_ButtonColumns - 1)) * -0.5f + sizeHalf;
        var buttonOffset = m_ButtonSize + m_ButtonSpacing;
        var sampleIndex = 0;
        for (var row = 0; row < m_ButtonRows; row++)
        {
            for (var col = 0; col < m_ButtonColumns; col++)
            {
                var button = Instantiate(m_SampleButtonPrefab, transform);
                var buttonTrans = button.transform;
                buttonTrans.localPosition = new Vector3(columnStart + buttonOffset * col, 0f, rowStart + buttonOffset * row);

                if (sampleIndex < m_Samples.Count)
                {
                    var sample = m_Samples[sampleIndex];
                    button.sampleClip = sample.sample;
                    button.midiNoteNumber = sample.midiNote.AsSevenBitNumber();
                    var useDisplayName = sample.displayName != null && sample.displayName.Length > 0;
                    button.text.text = useDisplayName ? sample.displayName : sample.midiNote.ToString();
                    button.interactable.selectEntered.AddListener(OnSampleButtonPressed);
                    button.interactable.selectExited.AddListener(OnSampleButtonReleased);
                    m_InteractableToButton[button.interactable] = button;
                }
                else
                {
                    button.text.text = "";
                }

                sampleIndex++;
            }
        }
    }

    void OnSampleButtonPressed(SelectEnterEventArgs args)
    {
        if (!isActiveAndEnabled || !m_InteractableToButton.TryGetValue(args.interactableObject, out var sampleButton))
            return;

        m_Amp.NoteOn(sampleButton.midiNoteNumber);
    }

    void OnSampleButtonReleased(SelectExitEventArgs args)
    {
        if (!isActiveAndEnabled || !m_InteractableToButton.TryGetValue(args.interactableObject, out var sampleButton))
            return;

        m_Amp.NoteOff(sampleButton.midiNoteNumber);
    }
}
