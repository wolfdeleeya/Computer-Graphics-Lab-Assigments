using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IKLegController : MonoBehaviour
{
    [SerializeField] private LayerMask _layer;

    [SerializeField] private Vector3 _rayDirection;
    [SerializeField] private float _distanceNotifier;
    [SerializeField] IKLegTarget _target;

    [Header("Gizmos")]
    [SerializeField] private float _gizmoWidth;

    private Transform _transform;
    private Vector3 _raycastPoint;
    private float _distanceToCheck;
    private bool _canMove;

    private void Awake()
    {
        _transform = transform;
        _distanceToCheck = Mathf.Pow(_distanceNotifier, 2);
        _canMove = true;
    }

    private void Start()
    {
        _target.StateChanged.AddListener(x => _canMove = x);
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        Physics.Raycast(_transform.position, _rayDirection, out hit, _layer);
        _raycastPoint = hit.point;
        if (LegMovementManager.Instance.CanMakeStep(_target) && _canMove && (_raycastPoint - _target.Transform.position).sqrMagnitude >= _distanceToCheck)
            _target.Move(_raycastPoint);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_raycastPoint, _gizmoWidth);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_target.Transform.position, _raycastPoint);
    }
}
