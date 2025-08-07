using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.Hands;

public class Tabletop : MonoBehaviour
{
    enum CalibrationState { Standby, Calibrating, Calibrated };

    [SerializeField]
    LineRenderer m_LineRendererPrefab;

    [SerializeField]
    Transform m_RightEdge;

    [SerializeField]
    Transform m_BackEdge;

    [SerializeField]
    Transform m_LeftEdge;

    [SerializeField]
    Transform m_FrontEdge;

    [SerializeField]
    Keyboard m_KeyboardPrefab;

    [SerializeField]
    Text m_DebugText;

    Vector3 m_InitialPosition;
    Quaternion m_InitialRotation;
    ARBoundingBox m_BoundingBox;
    float m_XScale;
    float m_ZScale;

    CalibrationState m_CalibrationState;
    LineRenderer m_CalibrationLineRenderer;

    Transform m_PrimaryHandPokeTransform;
    MetaSystemGestureDetector m_SecondaryHandGestureDetector;

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

        foreach (var pokeInteractor in FindObjectsByType<XRPokeInteractor>(FindObjectsSortMode.None))
        {
            if (pokeInteractor.handedness == InteractorHandedness.Right)
                m_PrimaryHandPokeTransform = pokeInteractor.attachTransform != null ? pokeInteractor.attachTransform : pokeInteractor.transform;
            else if (pokeInteractor.handedness == InteractorHandedness.Left)
                m_SecondaryHandGestureDetector = pokeInteractor.GetComponentInParent<MetaSystemGestureDetector>();
        }

        m_DebugText.text = string.Format("found right hand: {0}\nfound left hand: {1}", m_PrimaryHandPokeTransform != null, m_SecondaryHandGestureDetector != null);

        PrepareForCalibration();
    }

    void PrepareForCalibration()
    {
        m_CalibrationState = CalibrationState.Standby;
        transform.position = m_InitialPosition;
        transform.rotation = m_InitialRotation;
        m_SecondaryHandGestureDetector.indexPinchStarted.AddListener(OnSecondaryIndexPinchStarted);
    }

    void OnSecondaryIndexPinchStarted()
    {
        m_DebugText.text = "pinch";
        var pokePosition = m_PrimaryHandPokeTransform.position;
        if (m_CalibrationState == CalibrationState.Standby)
        {
            // make sure we're at the right table
            var pokeX = pokePosition.x;
            var pokeZ = pokePosition.z;
            var initX = m_InitialPosition.x;
            var initZ = m_InitialPosition.z;
            var halfXScale = m_XScale * 0.5f;
            var halfZScale = m_ZScale * 0.5f;
            if (pokeX >= initX - halfXScale && pokeX <= initX + halfXScale &&
                pokeZ >= initZ - halfZScale && pokeZ <= initZ + halfZScale)
            {
                StartCalibration(pokePosition);
            }
        }
        else
        {
            AddCalibrationPoint(pokePosition);
        }
    }

    void StartCalibration(Vector3 pokePosition)
    {
        m_DebugText.text = "start calibration";
        m_CalibrationState = CalibrationState.Calibrating;
        m_CalibrationLineRenderer = Instantiate(m_CalibrationLineRenderer);
        m_CalibrationLineRenderer.loop = false;
        m_CalibrationLineRenderer.useWorldSpace = true;
        m_CalibrationLineRenderer.positionCount = 2;
        m_CalibrationLineRenderer.SetPosition(0, pokePosition);
        m_CalibrationLineRenderer.SetPosition(1, pokePosition);
    }

    void AddCalibrationPoint(Vector3 point)
    {
        var pointCount = m_CalibrationLineRenderer.positionCount;
        m_CalibrationLineRenderer.SetPosition(pointCount - 1, point);
        if (pointCount < 3)
        {
            m_CalibrationLineRenderer.positionCount = 3;
            m_CalibrationLineRenderer.SetPosition(2, point);
            m_CalibrationLineRenderer.loop = true;
        }
        else
        {
            ConfirmCalibration();
        }
    }

    void ConfirmCalibration()
    {
        var points = new Vector3[3];
        m_CalibrationLineRenderer.GetPositions(points);
        var plane = new Plane(points[0], points[1], points[2]);
        if (Vector3.Dot(plane.normal, transform.up) < 0)
            plane.Flip();

        transform.position = plane.ClosestPointOnPlane(transform.position);
        var forwardProjection = Vector3.ProjectOnPlane(transform.forward, plane.normal);
        transform.LookAt(forwardProjection, plane.normal);

        m_CalibrationState = CalibrationState.Calibrated;
        CleanupCalibration();
    }

    void CleanupCalibration()
    {
        m_SecondaryHandGestureDetector.indexPinchStarted.RemoveListener(OnSecondaryIndexPinchStarted);
        if (m_CalibrationLineRenderer != null)
            Destroy(m_CalibrationLineRenderer);
    }

    void Update()
    {
        if (m_CalibrationLineRenderer != null && m_CalibrationState == CalibrationState.Calibrating)
        {
            // update end of line
            var pokePosition = m_PrimaryHandPokeTransform.position;
            m_CalibrationLineRenderer.SetPosition(m_CalibrationLineRenderer.positionCount - 1, pokePosition);
        }
    }

    public void ResetCalibration()
    {
        if (m_CalibrationState != CalibrationState.Calibrated)
            CleanupCalibration();

        PrepareForCalibration();
    }
}
