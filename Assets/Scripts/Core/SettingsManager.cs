using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField]
    Slider m_BpmSlider;

    [SerializeField]
    TextMeshProUGUI m_BpmValueText;

    [SerializeField]
    Slider m_TimeSigTopSlider;

    [SerializeField]
    TextMeshProUGUI m_TimeSigTopValueText;

    [SerializeField]
    Slider m_TimeSigBottomSlider;

    [SerializeField]
    TextMeshProUGUI m_TimeSigBottomValueText;

    [SerializeField]
    TMP_Dropdown m_RecordQuantizationDropdown;

    [SerializeField]
    Toggle m_RecordLeadInToggle;

    [SerializeField]
    Toggle m_PlayMetronomeToggle;

    [SerializeField]
    Vector2Int m_BpmRange = new Vector2Int(20, 200);

    [SerializeField]
    Vector2Int m_TimeSigTopRange = new Vector2Int(1, 15);

    void Start()
    {
        m_BpmSlider.wholeNumbers = true;
        m_BpmSlider.minValue = m_BpmRange.x;
        m_BpmSlider.maxValue = m_BpmRange.y;
        m_BpmSlider.value = TimingSettings.bpm;
        m_BpmValueText.text = TimingSettings.bpm.ToString("F0");

        m_TimeSigTopSlider.wholeNumbers = true;
        m_TimeSigTopSlider.minValue = m_TimeSigTopRange.x;
        m_TimeSigTopSlider.maxValue = m_TimeSigTopRange.y;
        m_TimeSigTopSlider.value = TimingSettings.timeSignature.numerator;
        m_TimeSigTopValueText.text = TimingSettings.timeSignature.numerator.ToString("F0");

        m_TimeSigBottomSlider.wholeNumbers = true;
        m_TimeSigBottomSlider.minValue = 0;
        m_TimeSigBottomSlider.maxValue = 4;
        m_TimeSigBottomSlider.value = math.log2(TimingSettings.timeSignature.denominator);
        m_TimeSigBottomValueText.text = TimingSettings.timeSignature.denominator.ToString("F0");

        m_RecordQuantizationDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var option in Enum.GetNames(typeof(Quantization)))
        {
            options.Add(new TMP_Dropdown.OptionData(option));
        }

        m_RecordQuantizationDropdown.AddOptions(options);
        m_RecordQuantizationDropdown.value = (int)TimingSettings.recordQuantization;

        m_RecordLeadInToggle.isOn = TimingSettings.recordLeadIn;

        m_PlayMetronomeToggle.isOn = TimingSettings.playMetronome;

        m_BpmSlider.onValueChanged.AddListener(SetBpm);
        m_TimeSigTopSlider.onValueChanged.AddListener(SetTimeSignatureNumerator);
        m_TimeSigBottomSlider.onValueChanged.AddListener(SetTimeSignatureDenominator);
        m_RecordQuantizationDropdown.onValueChanged.AddListener(SetRecordQuantization);
        m_RecordLeadInToggle.onValueChanged.AddListener(SetRecordLeadIn);
        m_PlayMetronomeToggle.onValueChanged.AddListener(SetPlayMetronome);
    }

    void OnDestroy()
    {
        m_BpmSlider.onValueChanged.RemoveListener(SetBpm);
        m_TimeSigTopSlider.onValueChanged.RemoveListener(SetTimeSignatureNumerator);
        m_TimeSigBottomSlider.onValueChanged.RemoveListener(SetTimeSignatureDenominator);
        m_RecordQuantizationDropdown.onValueChanged.RemoveListener(SetRecordQuantization);
        m_RecordLeadInToggle.onValueChanged.RemoveListener(SetRecordLeadIn);
        m_PlayMetronomeToggle.onValueChanged.RemoveListener(SetPlayMetronome);
    }

    void SetBpm(float value)
    {
        TimingSettings.bpm = value;
        m_BpmValueText.text = value.ToString("F0");
    }

    void SetTimeSignatureNumerator(float value)
    {
        TimingSettings.timeSignature.numerator = Mathf.RoundToInt(value);
        m_TimeSigTopValueText.text = value.ToString("F0");
    }

    void SetTimeSignatureDenominator(float value)
    {
        var exp = Mathf.RoundToInt(value);
        var denom = 1;
        for (var i = 0; i < exp; i++) denom *= 2;
        TimingSettings.timeSignature.denominator = denom;
        m_TimeSigBottomValueText.text = denom.ToString();
    }

    void SetRecordQuantization(int value)
    {
        TimingSettings.recordQuantization = (Quantization)value;
    }

    void SetRecordLeadIn(bool value)
    {
        TimingSettings.recordLeadIn = value;
    }

    void SetPlayMetronome(bool value)
    {
        TimingSettings.playMetronome = value;
    }
}
