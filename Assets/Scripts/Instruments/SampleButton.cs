using Melanchall.DryWetMidi.Common;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class SampleButton : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_Text;

    XRBaseInteractable m_Interactable;

    public XRBaseInteractable interactable
    {
        get
        {
            if (m_Interactable == null)
                m_Interactable = GetComponent<XRBaseInteractable>();
            return m_Interactable;
        }
    }

    public TextMeshProUGUI text => m_Text;

    public AudioClip sampleClip { get; set; }

    public SevenBitNumber midiNoteNumber { get; set; }

    void Start()
    {
        m_Interactable = GetComponent<XRBaseInteractable>();
    }
}
