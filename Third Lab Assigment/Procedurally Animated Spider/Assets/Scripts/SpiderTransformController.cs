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
    }
}
