using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;

public class TouchElement : MonoBehaviour
{
    public const string k_TouchLayerName = "Touch";

    public UnityEvent onTouchStart;
    public UnityEvent onTouchEnd;
    public UnityEvent onFirstTouchStarting;
    public UnityEvent onLastTouchEnded;

    HashSet<Collider> touchingColliders = new HashSet<Collider>();

    void Awake()
    {
        gameObject.SetLayerRecursively(LayerMask.NameToLayer(k_TouchLayerName));
    }

    void OnTriggerEnter(Collider other)
    {
        if (touchingColliders.Count == 0)
            OnFirstTouchStarting();

        touchingColliders.Add(other);
        onTouchStart.Invoke();
    }

    void OnTriggerExit(Collider other)
    {
        if (!touchingColliders.Contains(other))
            return;

        onTouchEnd.Invoke();
        touchingColliders.Remove(other);

        if (touchingColliders.Count == 0)
            OnLastTouchEnded();
    }

    void OnFirstTouchStarting()
    {
        onFirstTouchStarting.Invoke();
    }

    void OnLastTouchEnded()
    {
        onLastTouchEnded.Invoke();
    }
}
