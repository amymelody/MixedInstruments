using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Tabletop : MonoBehaviour
{
    const string k_TabletopYOffsetKeyFormat = "MixedInstruments/Tabletop/{0}/YOffset";

    [SerializeField]
    XRBaseInteractable m_CalibrationInteractablePrefab;

    [SerializeField]
    Text m_AdjustButtonText;

    [SerializeField]
    Transform m_RightEdge;

    [SerializeField]
    Transform m_BackEdge;

    [SerializeField]
    Transform m_LeftEdge;

    [SerializeField]
    Transform m_FrontEdge;

    [SerializeField]
    Text m_DebugText;

    Vector3 m_InitialPosition;
    Quaternion m_InitialRotation;
    ARBoundingBox m_BoundingBox;
    string m_YOffsetKey;
    float m_XScale;
    float m_ZScale;

    XRBaseInteractable m_CalibrationInteractable;
    XRPokeFollowAffordance m_CalibrationPokeFollowAffordance;
    bool m_Calibrating;

    InstrumentsSpawner m_InstrumentsSpawner;

    InstrumentsSpawner instrumentsSpawner
    {
        get
        {
            if (m_InstrumentsSpawner == null)
                m_InstrumentsSpawner = GetComponent<InstrumentsSpawner>();
            return m_InstrumentsSpawner;
        }
    }

    void Start()
    {
        m_InstrumentsSpawner = GetComponent<InstrumentsSpawner>();
    }

    public void SetupFromBoundingBox(ARBoundingBox boundingBox)
    {
        m_BoundingBox = boundingBox;
        m_YOffsetKey = string.Format(k_TabletopYOffsetKeyFormat, boundingBox.trackableId);

        var tablePose = boundingBox.pose;
        // offset a bit to give room to push down for alignment
        const float offset = 0.05f;
        var top = tablePose.position + tablePose.rotation * ((0.5f * boundingBox.size.y + offset) * Vector3.up);
        transform.position = top;
        transform.rotation = tablePose.rotation;
        m_InitialPosition = top;
        m_InitialRotation = tablePose.rotation;

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
        Debug.Log("SET TABLETOP DIMENSIONS");

        if (PlayerPrefs.HasKey(m_YOffsetKey))
        {
            Debug.Log("RESTORE TABLETOP " + boundingBox.trackableId);
            var yOffset = PlayerPrefs.GetFloat(m_YOffsetKey);
            transform.position += Vector3.up * yOffset;
        }

        //InitiateCalibration();
        //instrumentsSpawner.Spawn(m_XScale, m_ZScale);
    }

    public void ToggleCalibrationMode()
    {
        if (m_Calibrating)
            ConfirmCalibration();
        else
            InitiateCalibration();
    }

    void InitiateCalibration()
    {
        m_Calibrating = true;
        m_AdjustButtonText.text = "Confirm";
        m_CalibrationInteractable = Instantiate(m_CalibrationInteractablePrefab, transform);

        m_CalibrationPokeFollowAffordance = m_CalibrationInteractable.GetComponentInChildren<XRPokeFollowAffordance>();
        if (m_CalibrationPokeFollowAffordance == null)
        {
            Debug.LogError("Tabletop calibration interactable must have XRPokeFollowAffordance");
            enabled = false;
            return;
        }

        m_CalibrationInteractable.transform.localScale = new Vector3(m_XScale, m_CalibrationInteractable.transform.localScale.y, m_ZScale);
    }

    void ConfirmCalibration()
    {
        var yOffset = m_CalibrationPokeFollowAffordance.pokeFollowTransform.position.y - m_InitialPosition.y;
        transform.position = m_InitialPosition + Vector3.up * yOffset;
        PlayerPrefs.SetFloat(m_YOffsetKey, yOffset);
        CleanupCalibration();

        //instrumentsSpawner.Spawn(m_XScale, m_ZScale);
    }

    void CleanupCalibration()
    {
        m_Calibrating = false;
        m_AdjustButtonText.text = "Adjust";
        if (m_CalibrationInteractable != null)
            Destroy(m_CalibrationInteractable.gameObject);
    }

    public void ResetCalibration()
    {
        CleanupCalibration();
        transform.position = m_InitialPosition;
        //instrumentsSpawner.Despawn();
    }
}
