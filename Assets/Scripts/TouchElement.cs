using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TouchElement : MonoBehaviour
{
    public UnityEvent onTouchStart;
    public UnityEvent onTouchEnd;
    public UnityEvent onFirstTouchStarting;
    public UnityEvent onLastTouchEnded;

    HashSet<IXRInteractor> touchingInteractors = new HashSet<IXRInteractor>();

    void Awake()
    {
        gameObject.SetLayerRecursively(LayerMask.NameToLayer(TouchSurface.k_TouchLayerName));
    }

    public void OnTouchStart(IXRInteractor interactor)
    {
        if (touchingInteractors.Count == 0)
            OnFirstTouchStarting(interactor);

        touchingInteractors.Add(interactor);
        onTouchStart.Invoke();
    }

    public void OnTouchEnd(IXRInteractor interactor)
    {
        if (!touchingInteractors.Contains(interactor))
            return;

        onTouchEnd.Invoke();
        touchingInteractors.Remove(interactor);

        if (touchingInteractors.Count == 0)
            OnLastTouchEnded(interactor);
    }

    void OnFirstTouchStarting(IXRInteractor interactor)
    {
        onFirstTouchStarting.Invoke();
    }

    void OnLastTouchEnded(IXRInteractor interactor)
    {
        onLastTouchEnded.Invoke();
    }
}
