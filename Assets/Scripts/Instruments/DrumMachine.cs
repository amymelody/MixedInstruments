using Melanchall.DryWetMidi.Standards;
using System;
using System.Collections.Generic;
using UnityEngine;

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
    List<DrumSample> m_Samples;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
