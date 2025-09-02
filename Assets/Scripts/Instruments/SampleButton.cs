using Melanchall.DryWetMidi.Common;
using TMPro;
using UnityEngine;

public class SampleButton : TouchElement
{
    [SerializeField]
    TextMeshProUGUI m_Text;

    public TextMeshProUGUI text => m_Text;

    public AudioClip sampleClip { get; set; }

    public SevenBitNumber midiNoteNumber { get; set; }
}
