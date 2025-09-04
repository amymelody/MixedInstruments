using UnityEngine;

public static class TimingUtils
{
    static Metronome k_Metronome;
    public static Metronome metronome
    {
        get
        {
            if (k_Metronome == null)
                k_Metronome = Object.FindFirstObjectByType<Metronome>();
            return k_Metronome;
        }
    }

    public static long GetBar()
    {
        return metronome.bar;
    }

    public static float GetBarPhase()
    {
        return metronome.barPhase;
    }

    public static long GetTick()
    {
        return metronome.tick;
    }
}
