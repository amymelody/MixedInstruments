using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField]
    Slider m_BpmSlider;

    [SerializeField]
    TextMeshProUGUI m_BpmValueText;

    [SerializeField]
    TMP_Dropdown m_RecordQuantizationDropdown;

    [SerializeField]
    Toggle m_RecordLeadInToggle;

    [SerializeField]
    Toggle m_PlayMetronomeToggle;

    [SerializeField]
    Vector2Int m_BpmRange = new Vector2Int(20, 200);

    void Start()
    {
        m_BpmSlider.wholeNumbers = true;
        m_BpmSlider.minValue = m_BpmRange.x;
        m_BpmSlider.maxValue = m_BpmRange.y;
        m_BpmSlider.value = TimingSettings.bpm;
        m_BpmValueText.text = TimingSettings.bpm.ToString("F0");

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
        m_RecordQuantizationDropdown.onValueChanged.AddListener(SetRecordQuantization);
        m_RecordLeadInToggle.onValueChanged.AddListener(SetRecordLeadIn);
        m_PlayMetronomeToggle.onValueChanged.AddListener(SetPlayMetronome);
    }

    void OnDestroy()
    {
        m_BpmSlider.onValueChanged.RemoveListener(SetBpm);
        m_RecordQuantizationDropdown.onValueChanged.RemoveListener(SetRecordQuantization);
        m_RecordLeadInToggle.onValueChanged.RemoveListener(SetRecordLeadIn);
        m_PlayMetronomeToggle.onValueChanged.RemoveListener(SetPlayMetronome);
    }

    void SetBpm(float value)
    {
        TimingSettings.bpm = value;
        m_BpmValueText.text = value.ToString("F0");
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
