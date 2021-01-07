using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderTransformController : MonoBehaviour
{
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void LateUpdate()
    {
        Vector3 position = _transform.position;
        _transform.position = new Vector3(position.x, LegMovementManager.Instance.GetSpiderY(), position.z);
        _transform.rotation = Quaternion.Euler(LegMovementManager.Instance.GetEulerSpiderRotation());
    }
}
