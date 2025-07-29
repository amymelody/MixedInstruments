using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Tabletop : MonoBehaviour
{
    [SerializeField]
    XRBaseInteractable m_CalibrationInteractable;

    [SerializeField]
    float m_CalibrationDelayTime = 2f;

    XRPokeFollowAffordance m_CalibrationPokeFollowAffordance;
    float m_CalibrationStartTime = -1f;

    public void SetupFromBoundingBox(ARBoundingBox boundingBox)
    {
        m_CalibrationPokeFollowAffordance = m_CalibrationInteractable.GetComponentInChildren<XRPokeFollowAffordance>();
        if (m_CalibrationPokeFollowAffordance == null)
        {
            Debug.LogError("Tabletop calibration interactable must have XRPokeFollowAffordance");
            enabled = false;
            return;
        }

        var tablePose = boundingBox.pose;
        // offset a bit to give room to push down for alignment
        const float offset = 0.05f;
        var top = tablePose.position + tablePose.rotation * ((0.5f * boundingBox.size.y + offset) * Vector3.up);
        transform.position = top;
        transform.rotation = tablePose.rotation;

        m_CalibrationInteractable.transform.localScale = new Vector3(boundingBox.size.x, m_CalibrationInteractable.transform.localScale.y, boundingBox.size.z);

        // Confirm calibration after first poke ends, with delay to accomodate adjustments
        m_CalibrationInteractable.hoverExited.AddListener(OnCalibrationHoverEnded);
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
        Destroy(m_CalibrationInteractable.gameObject);
    }
}
