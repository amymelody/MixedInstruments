using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TabletopManager : MonoBehaviour
{
    [SerializeField]
    Tabletop m_TabletopPrefab;

    [SerializeField]
    List<Module> m_ModulePrefabs;

    ARBoundingBoxManager m_BoundingBoxManager;
    int m_TabletopCount;

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
            SpawnTabletop(boundingBox);
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
            SpawnTabletop(boundingBox);
        }
    }

    void SpawnTabletop(ARBoundingBox boundingBox)
    {
        if (!boundingBox.classifications.HasFlag(UnityEngine.XR.ARSubsystems.BoundingBoxClassifications.Table))
            return;

        var tabletop = Instantiate(m_TabletopPrefab);
        var modulePrefab = m_TabletopCount < m_ModulePrefabs.Count ? m_ModulePrefabs[m_TabletopCount] : null;
        m_TabletopCount++;
        tabletop.SetupFromBoundingBox(boundingBox, modulePrefab);
    }
}
