using UnityEngine;

public static class TransformExtensions
{
    public static void SetXScale(this Transform transform, float scale)
    {
        transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);
    }

    public static void SetZScale(this Transform transform, float scale)
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, scale);
    }

    public static void SetXPosition(this Transform transform, float position)
    {
        transform.localPosition = new Vector3(position, transform.localPosition.y, transform.localPosition.z);
    }

    public static void SetZPosition(this Transform transform, float position)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, position);
    }
}
