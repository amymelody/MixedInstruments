using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TouchSurface : XRBaseInteractable
{
    public const string k_TouchLayerName = "Touch";

    LayerMask m_TouchLayerMask;

    [SerializeField]
    float m_ElementOverlapRadius = 0.015f;

    static readonly Collider[] s_Colliders = new Collider[16];

    Dictionary<IXRInteractor, TouchElement> m_InteractorTouches = new Dictionary<IXRInteractor, TouchElement>();

    protected override void Awake()
    {
        base.Awake();
        m_TouchLayerMask = LayerMask.GetMask(k_TouchLayerName);
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // overlapsphere to find which elements we're close to, then compare closestpointonbounds to find the closest one
        // TODO: could optimize for flat surface
        var interactor = args.interactorObject;
        var interactionPosition = interactor.GetAttachTransform(this).position;
        var overlapCount = Physics.OverlapSphereNonAlloc(interactionPosition, m_ElementOverlapRadius, s_Colliders, m_TouchLayerMask, QueryTriggerInteraction.Collide);
        var minSqDist = float.MaxValue;
        Collider closestCollider = null;
        for (var i = 0; i < overlapCount; ++i)
        {
            var closestPoint = s_Colliders[i].ClosestPointOnBounds(interactionPosition);
            var sqDist = (closestPoint - interactionPosition).sqrMagnitude;
            if (sqDist < minSqDist)
            {
                minSqDist = sqDist;
                closestCollider = s_Colliders[i];
            }
        }

        if (closestCollider == null)
            return;

        var touchElement = closestCollider.GetComponentInParent<TouchElement>();
        if (touchElement != null)
        {
            if (m_InteractorTouches.TryGetValue(interactor, out var lastTouch) && lastTouch != null)
                lastTouch.OnTouchEnd(interactor);

            m_InteractorTouches[interactor] = touchElement;
            touchElement.OnTouchStart(interactor);
        }
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);

        var interactor = args.interactorObject;
        if (m_InteractorTouches.TryGetValue(interactor, out var touchElement) && touchElement != null)
        {
            touchElement.OnTouchEnd(interactor);
            m_InteractorTouches[interactor] = null;
        }
    }
}
