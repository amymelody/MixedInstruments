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
    public static float bpm
    {
        get => k_Bpm;
        set => k_Bpm = value;
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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        k_Bpm = PlayerPrefs.HasKey(k_BpmKey) ? PlayerPrefs.GetFloat(k_BpmKey) : 120f;
        k_RecordQuantization = PlayerPrefs.HasKey(k_RecordQuantizationKey) ? (Quantization)PlayerPrefs.GetInt(k_RecordQuantizationKey) : Quantization.None;
        k_RecordLeadIn = PlayerPrefs.HasKey(k_RecordLeadInKey) ? PlayerPrefs.GetInt(k_RecordLeadInKey) > 0 : false;
        k_PlayMetronome = PlayerPrefs.HasKey(k_PlayMetronomeKey) ? PlayerPrefs.GetInt(k_PlayMetronomeKey) > 0 : false;

        Application.quitting += OnApplicationQuitting;
    }

    static void OnApplicationQuitting()
    {
        PlayerPrefs.SetFloat(k_BpmKey, k_Bpm);
        PlayerPrefs.SetInt(k_RecordQuantizationKey, (int)k_RecordQuantization);
        PlayerPrefs.SetInt(k_RecordLeadInKey, k_RecordLeadIn ? 1 : 0);
        PlayerPrefs.SetInt(k_PlayMetronomeKey, k_PlayMetronome ? 1 : 0);
    }
}
