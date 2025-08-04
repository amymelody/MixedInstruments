using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Tabletop : MonoBehaviour
{
    [SerializeField]
    XRBaseInteractable m_CalibrationInteractablePrefab;

    [SerializeField]
    float m_CalibrationDelayTime = 2f;

    [SerializeField]
    Transform m_RightEdge;

    [SerializeField]
    Transform m_BackEdge;

    [SerializeField]
    Transform m_LeftEdge;

    [SerializeField]
    Transform m_FrontEdge;

    Vector3 m_InitialPosition;
    ARBoundingBox m_BoundingBox;
    float m_XScale;
    float m_ZScale;

    XRBaseInteractable m_CalibrationInteractable;
    XRPokeFollowAffordance m_CalibrationPokeFollowAffordance;
    float m_CalibrationStartTime = -1f;

    Transform m_TopEdges;

    public void SetupFromBoundingBox(ARBoundingBox boundingBox)
    {
        m_BoundingBox = boundingBox;

        var tablePose = boundingBox.pose;
        // offset a bit to give room to push down for alignment
        const float offset = 0.05f;
        var top = tablePose.position + tablePose.rotation * ((0.5f * boundingBox.size.y + offset) * Vector3.up);
        transform.position = top;
        transform.rotation = tablePose.rotation;
        m_InitialPosition = top;

        m_XScale = boundingBox.size.x;
        m_ZScale = boundingBox.size.z;
        m_RightEdge.SetZScale(m_ZScale);
        m_RightEdge.SetXPosition(m_XScale * 0.5f);
        m_BackEdge.SetXScale(m_XScale);
        m_BackEdge.SetZPosition(m_ZScale * 0.5f);
        m_LeftEdge.SetZScale(m_ZScale);
        m_LeftEdge.SetXPosition(m_XScale * -0.5f);
        m_FrontEdge.SetXScale(m_XScale);
        m_FrontEdge.SetZPosition(m_ZScale * -0.5f);
        m_TopEdges = m_RightEdge.parent;

        InitiateCalibration();
    }

    void InitiateCalibration()
    {
        m_CalibrationStartTime = -1f;
        m_CalibrationInteractable = Instantiate(m_CalibrationInteractablePrefab, transform);

        m_CalibrationPokeFollowAffordance = m_CalibrationInteractable.GetComponentInChildren<XRPokeFollowAffordance>();
        if (m_CalibrationPokeFollowAffordance == null)
        {
            Debug.LogError("Tabletop calibration interactable must have XRPokeFollowAffordance");
            enabled = false;
            return;
        }

        m_CalibrationInteractable.transform.localScale = new Vector3(m_XScale, m_CalibrationInteractable.transform.localScale.y, m_ZScale);

        // Confirm calibration after first poke ends, with delay to accomodate adjustments
        m_CalibrationInteractable.hoverExited.AddListener(OnCalibrationHoverEnded);

        transform.position = m_InitialPosition;
        m_TopEdges.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_CalibrationStartTime > 0f && Time.time - m_CalibrationStartTime >= m_CalibrationDelayTime)
            ConfirmCalibration();
    }

    void OnCalibrationHoverEnded(HoverExitEventArgs args)
    {
        m_CalibrationStartTime = Time.time;
    }

    void ConfirmCalibration()
    {
        m_CalibrationStartTime = -1f;
        transform.position = new Vector3(transform.position.x, m_CalibrationPokeFollowAffordance.pokeFollowTransform.position.y, transform.position.z);
        m_TopEdges.gameObject.SetActive(true);
        Destroy(m_CalibrationInteractable.gameObject);
    }

    public void ResetCalibration()
    {
        if (m_CalibrationInteractable != null)
            Destroy(m_CalibrationInteractable.gameObject);

        InitiateCalibration();
    }
}
