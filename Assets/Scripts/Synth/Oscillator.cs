using UnityEngine;

public class Oscillator
{
    public float Sample(float phase)
    {
        return Mathf.Sin(phase * MathUtils.TwoPi);
    }
}
