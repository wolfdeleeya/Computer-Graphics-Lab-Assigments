using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SegmentMovementController : MonoBehaviour
{
    [SerializeField] private List<IKLegTarget> _segmentTargets;

    [Header("Gizmo Properties")]
    [SerializeField] private float _gizmoWidth;
    [SerializeField] private float _gizmoLength;
    [SerializeField] private Color _lineColor;
    [SerializeField] private Color _sphereColor;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void LateUpdate()
    {
        Vector3 pos = _transform.position;
        _transform.position = new Vector3(pos.x, GetSegmentY(), pos.z);
    }

    public float GetSegmentY()
    {
        float ySum = 0;
        foreach (IKLegTarget target in _segmentTargets)
            ySum += target.Transform.position.y;
        return ySum / _segmentTargets.Count + LegMovementManager.Instance.GetBaseHeight();
    }
}
