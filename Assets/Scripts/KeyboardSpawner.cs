using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class KeyboardSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject m_KeyboardPrefab;

    ARBoundingBoxManager m_BoundingBoxManager;

    void OnEnable()
    {
        m_BoundingBoxManager = FindAnyObjectByType<ARBoundingBoxManager>();
        if (m_BoundingBoxManager == null)
        {
            Debug.LogError("Need ARBoundingBoxManager in scene");
            enabled = false;
            return;
        }

        m_BoundingBoxManager.trackablesChanged.AddListener(OnBoundingBoxesChanged);
        foreach (var boundingBox in m_BoundingBoxManager.trackables)
        {
            SpawnKeyboardOnTable(boundingBox);
        }
    }

    void OnDisable()
    {
        if (m_BoundingBoxManager == null)
            return;

        m_BoundingBoxManager.trackablesChanged.RemoveListener(OnBoundingBoxesChanged);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnBoundingBoxesChanged(ARTrackablesChangedEventArgs<ARBoundingBox> args)
    {
        foreach (var boundingBox in args.added)
        {
            SpawnKeyboardOnTable(boundingBox);
        }
    }

    void SpawnKeyboardOnTable(ARBoundingBox boundingBox)
    {
        if (!boundingBox.classifications.HasFlag(UnityEngine.XR.ARSubsystems.BoundingBoxClassifications.Table))
            return;

        var tablePose = boundingBox.pose;
        var top = tablePose.position + tablePose.rotation * (0.5f * boundingBox.size.y * Vector3.up);
        Instantiate(m_KeyboardPrefab, top, tablePose.rotation);
    }
}
