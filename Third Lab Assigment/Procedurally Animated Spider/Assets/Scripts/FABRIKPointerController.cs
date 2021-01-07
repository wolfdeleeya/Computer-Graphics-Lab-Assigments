using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FABRIKPointerController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _anklePoint;
    [SerializeField, Range(0, 1)] private float _tolerance;

    private List<Transform> _bones;
    private List<Vector3> _positions1;
    private List<Vector3> _positions2;
    private List<float> _boneLengths;
    private float _totalLength;

    private int _indexOfTarget;

    private void Awake()
    {
        _bones = new List<Transform>();
        _boneLengths = new List<float>();
        _positions1 = new List<Vector3>();
        _positions2 = new List<Vector3>();
    }

    private void Start()
    {
        Transform currentBone = transform;

        while (currentBone != null && currentBone.CompareTag("Bone"))
        {
            _bones.Insert(0, currentBone);
            currentBone = currentBone.parent;
        }

        _indexOfTarget = _bones.Count - 1;

        //Calculating Bone Lengths And Total Bone Length
        _totalLength = 0;
        for (int i = 1; i < _bones.Count; ++i) 
        {
            float currentBoneLength = (_bones[i].position - _bones[i - 1].position).magnitude;
            _boneLengths.Add(currentBoneLength);
            _totalLength += currentBoneLength;
        }
    }

    private void LateUpdate()
    {
        CalculateFABRIK();
    }

    private void CalculateFABRIK()
    {
        Vector3 rootPosition = _bones[0].transform.position;

        if ((rootPosition - _target.position).magnitude > _totalLength)
        {
            SetDefaultRotations();
            return;
        }

        foreach (Transform bone in _bones)
        {
            _positions1.Add(bone.position);
            _positions2.Add(Vector3.zero);
        }

        while ((_positions1[_indexOfTarget] - _target.position).sqrMagnitude > _tolerance)
        {
            _positions2[_indexOfTarget] = _target.position;

            //Forwards Pass
            for (int i = _indexOfTarget - 1; i >= 0; --i)
            {
                Vector3 deltaPosition = (_positions1[i] - _positions2[i + 1]).normalized * _boneLengths[i];
                _positions2[i] = _positions2[i+1] + deltaPosition;
            }

            _positions1[0] = rootPosition;

            //Backwards Pass
            for (int i = 1; i < _bones.Count; ++i)
            {
                Vector3 deltaPosition = (_positions2[i] - _positions1[i - 1]).normalized * _boneLengths[i-1];
                _positions1[i] = _positions1[i - 1] + deltaPosition;
            }
        }

        //Bend Towards The Ankle Point If There Is One
        if (_anklePoint != null)
            for (int i = 1; i < _bones.Count - 1; ++i)
            {
                Plane plane = new Plane(_positions1[i+1]-_positions1[i-1],_positions1[i-1]);
                
                Vector3 bonePlanePoint = plane.ClosestPointOnPlane(_bones[i].position);
                Vector3 anklePlanePoint = plane.ClosestPointOnPlane(_anklePoint.position);

                float rotation = Vector3.SignedAngle(bonePlanePoint - _positions1[i - 1], anklePlanePoint - _positions1[i - 1], plane.normal);
                _positions1[i] = Quaternion.AngleAxis(rotation, plane.normal) * (_positions1[i] - _positions1[i - 1]) + _positions1[i - 1];
            }

        for (int i = 0; i < _bones.Count - 1; ++i)
        {
            Vector3 lookVector = (_positions1[i + 1] - _bones[i].position).normalized;
            _bones[i].rotation = Quaternion.LookRotation(lookVector);
        }

        _positions1.Clear();
        _positions2.Clear();
    }

    private void SetDefaultRotations() 
    {
        Vector3 lookVector = _target.position - _bones[0].position;
        for (int i = 0; i < _bones.Count - 1; ++i)
        {
            _bones[i].rotation = Quaternion.LookRotation(lookVector);
        }
    }
}
