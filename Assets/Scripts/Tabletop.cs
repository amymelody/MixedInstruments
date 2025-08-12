using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.Hands;

public class Tabletop : MonoBehaviour
{
    enum CalibrationState { Standby, Calibrating, Calibrated };

    [SerializeField]
    bool m_IsolationTest;

    [SerializeField]
    LineRenderer m_LineRendererPrefab;

    [SerializeField]
    float m_PointSampleFrequency = 0.05f;

    [SerializeField]
    float m_MinPointSeparation = 0.002f;

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
    float m_XScale;
    float m_ZScale;

    CalibrationState m_CalibrationState;
    LineRenderer m_CalibrationLineRenderer;
    float m_LastSampleTime;
    Vector3 m_LastPoint;

    Transform m_PrimaryHandPokeTransform;
    MetaSystemGestureDetector m_SecondaryHandGestureDetector;

    InstrumentsSpawner m_InstrumentsSpawner;

    void Start()
    {
        m_InstrumentsSpawner = GetComponent<InstrumentsSpawner>();
    }

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
            ConfirmCalibration();
        }
    }

    void StartCalibration(Vector3 pokePosition)
    {
        m_DebugText.text = "start calibration";
        m_CalibrationState = CalibrationState.Calibrating;
        m_CalibrationLineRenderer = Instantiate(m_LineRendererPrefab);
        m_CalibrationLineRenderer.loop = false;
        m_CalibrationLineRenderer.useWorldSpace = true;
        m_LastSampleTime = Time.time;
        m_LastPoint = pokePosition;
        m_CalibrationLineRenderer.positionCount = 1;
        m_CalibrationLineRenderer.SetPosition(0, pokePosition);
    }

    void Update()
    {
        if (m_CalibrationState == CalibrationState.Calibrating &&
            Time.time - m_LastSampleTime >= m_PointSampleFrequency)
        {
            AddCalibrationPoint(m_PrimaryHandPokeTransform.position);
        }
    }

    void AddCalibrationPoint(Vector3 point)
    {
        m_LastSampleTime = Time.time;
        if ((point - m_LastPoint).sqrMagnitude < m_MinPointSeparation * m_MinPointSeparation)
            return;

        m_LastPoint = point;
        m_CalibrationLineRenderer.positionCount++;
        m_CalibrationLineRenderer.SetPosition(m_CalibrationLineRenderer.positionCount - 1, point);
    }

    void ConfirmCalibration()
    {
        var points = new Vector3[m_CalibrationLineRenderer.positionCount];
        m_CalibrationLineRenderer.GetPositions(points);
        var plane = MathUtils.FitPlane(points, out var discardedPointCount);
        m_DebugText.text = string.Format("discarded points: {0}", discardedPointCount);
        if (Vector3.Dot(plane.normal, transform.up) < 0)
            plane.Flip();

        transform.position = plane.ClosestPointOnPlane(transform.position);
        var forwardProjection = Vector3.ProjectOnPlane(transform.forward, plane.normal);
        transform.LookAt(transform.position + forwardProjection * 100f, plane.normal);

        m_CalibrationState = CalibrationState.Calibrated;
        CleanupCalibration();

        m_InstrumentsSpawner?.Spawn(m_XScale, m_ZScale);
    }

    void CleanupCalibration()
    {
        m_SecondaryHandGestureDetector.indexPinchStarted.RemoveListener(OnSecondaryIndexPinchStarted);
        if (m_CalibrationLineRenderer != null)
            Destroy(m_CalibrationLineRenderer);
    }

    public void ResetCalibration()
    {
        if (m_CalibrationState != CalibrationState.Calibrated)
            CleanupCalibration();

        m_InstrumentsSpawner?.Despawn();
        PrepareForCalibration();
    }
}
