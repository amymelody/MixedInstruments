using Melanchall.DryWetMidi.Common;
using UnityEngine;

public class SampleButton : TouchElement
{
    public AudioClip sampleClip { get; set; }

    public SevenBitNumber midiNoteNumber { get; set; }
}
