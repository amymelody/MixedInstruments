using UnityEngine;

[RequireComponent(typeof(FreeOscillatorAmp))]
public class OscLerpController : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float m_LerpValue;

    FreeOscillatorAmp m_Amp;

    void Awake()
    {
        m_Amp = GetComponent<FreeOscillatorAmp>();
    }

    void Update()
    {
        m_Amp.LerpFrequency(m_LerpValue);
    }
}
