using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class TouchElement : MonoBehaviour
{
    public const string k_TouchLayerName = "TouchElement";

    [SerializeField]
    BoxCollider m_BoxCollider;

    [SerializeField]
    TextMeshProUGUI m_Text;

    [SerializeField]
    Renderer m_Renderer;

    public TextMeshProUGUI text => m_Text;

    public Renderer buttonRenderer => m_Renderer;

    public UnityEvent<TouchElement> onTouchStart;
    public UnityEvent<TouchElement> onTouchEnd;

    Collider m_TouchingCollider;

    void Awake()
    {
        gameObject.SetLayerRecursively(LayerMask.NameToLayer(k_TouchLayerName));
    }

    void Update()
    {
        if (m_TouchingCollider != null)
        {
            var colliderTrans = m_BoxCollider.transform;
            var top = colliderTrans.localPosition.y + m_BoxCollider.center.y + m_BoxCollider.size.y * 0.5f;
            var pointerY = colliderTrans.parent.InverseTransformPoint(m_TouchingCollider.transform.position).y;
            if (pointerY < top)
            {
                colliderTrans.localPosition += Vector3.up * (pointerY - top);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (m_TouchingCollider != null)
            return;

        m_TouchingCollider = other;
        onTouchStart.Invoke(this);
    }

    void OnTriggerExit(Collider other)
    {
        if (other != m_TouchingCollider)
            return;

        onTouchEnd.Invoke(this);
        m_TouchingCollider = null;
    }
}
