using Melanchall.DryWetMidi.Interaction;
using UnityEngine;

public enum Quantization
{
    None,
    Quarter,
    Eighth,
    EighthTriplets,
    EighthAndTriplets,
    Sixteenth,
    SixteenthTriplets,
    SixteenthAndTriplets,
    ThirtySecond
}

public static class TimingSettings
{
    public const string PlayerPrefsPrefix = AppConstants.PlayerPrefsPrefix + "Timing/";

    const string k_BpmKey = PlayerPrefsPrefix + "BPM";
    static float k_Bpm;

    /// <summary>
    /// Quarter notes per minute
    /// </summary>
    public static float bpm
    {
        get => k_Bpm;
        set => k_Bpm = value;
    }

    const string k_TimeSignatureNumeratorKey = PlayerPrefsPrefix + "Numerator";
    const string k_TimeSignatureDenominatorKey = PlayerPrefsPrefix + "Denominator";
    static TimeSignature k_TimeSignature;
    public static TimeSignature timeSignature
    {
        get => k_TimeSignature;
        set => k_TimeSignature = value;
    }

    const string k_RecordQuantizationKey = PlayerPrefsPrefix + "RecordQuantization";
    static Quantization k_RecordQuantization;
    public static Quantization recordQuantization
    {
        get => k_RecordQuantization;
        set => k_RecordQuantization = value;
    }

    const string k_RecordLeadInKey = PlayerPrefsPrefix + "RecordLeadIn";
    static bool k_RecordLeadIn;
    public static bool recordLeadIn
    {
        get => k_RecordLeadIn;
        set => k_RecordLeadIn = value;
    }

    const string k_PlayMetronomeKey = PlayerPrefsPrefix + "PlayMetronome";
    static bool k_PlayMetronome;
    public static bool playMetronome
    {
        get => k_PlayMetronome;
        set => k_PlayMetronome = value;
    }

    public static bool recordLeadInActive { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        k_Bpm = PlayerPrefs.HasKey(k_BpmKey) ? PlayerPrefs.GetFloat(k_BpmKey) : 120f;
        k_RecordQuantization = PlayerPrefs.HasKey(k_RecordQuantizationKey) ? (Quantization)PlayerPrefs.GetInt(k_RecordQuantizationKey) : Quantization.None;
        k_RecordLeadIn = PlayerPrefs.HasKey(k_RecordLeadInKey) ? PlayerPrefs.GetInt(k_RecordLeadInKey) > 0 : false;
        k_PlayMetronome = PlayerPrefs.HasKey(k_PlayMetronomeKey) ? PlayerPrefs.GetInt(k_PlayMetronomeKey) > 0 : false;
        var numerator = PlayerPrefs.HasKey(k_TimeSignatureNumeratorKey) ? PlayerPrefs.GetInt(k_TimeSignatureNumeratorKey) : 4;
        var denominator = PlayerPrefs.HasKey(k_TimeSignatureDenominatorKey) ? PlayerPrefs.GetInt(k_TimeSignatureDenominatorKey) : 4;
        k_TimeSignature = new TimeSignature(numerator, denominator);

        Application.quitting += OnApplicationQuitting;
    }

    static void OnApplicationQuitting()
    {
        PlayerPrefs.SetFloat(k_BpmKey, k_Bpm);
        PlayerPrefs.SetInt(k_RecordQuantizationKey, (int)k_RecordQuantization);
        PlayerPrefs.SetInt(k_RecordLeadInKey, k_RecordLeadIn ? 1 : 0);
        PlayerPrefs.SetInt(k_PlayMetronomeKey, k_PlayMetronome ? 1 : 0);
        PlayerPrefs.SetInt(k_TimeSignatureNumeratorKey, k_TimeSignature.Numerator);
        PlayerPrefs.SetInt(k_TimeSignatureDenominatorKey, k_TimeSignature.Denominator);
    }
}
