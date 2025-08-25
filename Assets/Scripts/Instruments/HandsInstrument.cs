using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands.Samples.GestureSample;

public class HandsInstrument : MonoBehaviour
{
    class GestureCallbackReceiver
    {
        public Note note;
        public MidiSynthAmp midiInstrument;

        public GestureCallbackReceiver(Note note, MidiSynthAmp midiInstrument)
        {
            this.note = note;
            this.midiInstrument = midiInstrument;
        }
        
        public void OnGesturePerformed()
        {
            midiInstrument.NoteOn(note.NoteNumber);
        }

        public void OnGestureEnded()
        {
            midiInstrument.NoteOff(note.NoteNumber);
        }
    }

    Dictionary<SevenBitNumber, GestureCallbackReceiver> m_GestureCallbackReceivers = new Dictionary<SevenBitNumber, GestureCallbackReceiver> ();

    void Awake()
    {
        var midiInstrument = GetComponentInChildren<MidiSynthAmp>();
        foreach (var gestureRecognizer in GetComponentsInChildren<StaticHandGesture>())
        {
            var noteTag = gestureRecognizer.GetComponent<NoteTag>();
            if (noteTag == null)
                continue;

            var note = Note.Get(noteTag.NoteName, noteTag.NoteOctave);
            var noteNumber = note.NoteNumber;
            if (!m_GestureCallbackReceivers.TryGetValue(noteNumber, out var receiver))
            {
                receiver = new GestureCallbackReceiver(note, midiInstrument);
                m_GestureCallbackReceivers[noteNumber] = receiver;
            }

            gestureRecognizer.gesturePerformed.AddListener(receiver.OnGesturePerformed);
            gestureRecognizer.gestureEnded.AddListener(receiver.OnGestureEnded);
        }
    }
}
