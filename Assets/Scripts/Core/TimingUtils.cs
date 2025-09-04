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
}
