using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderTransformController : MonoBehaviour
{
    [SerializeField] private float _movementSpeed;

    private Vector3 _movementDirection;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _movementDirection = Vector3.zero;
    }

    public void SetMovementDirection(Vector2 movementDirection) => _movementDirection = new Vector3(movementDirection.x, 0, movementDirection.y);

    private void LateUpdate()
    {
        Vector3 position = _transform.position + _movementDirection*_movementSpeed*Time.deltaTime;
        _transform.position = new Vector3(position.x, LegMovementManager.Instance.GetSpiderY(), position.z);
        Vector3 rotation = LegMovementManager.Instance.CalculateLegVectors(out Vector3 vec1, out Vector3 vec2).eulerAngles;
        rotation = new Vector3(rotation.x, _transform.rotation.eulerAngles.y, rotation.z);
        _transform.rotation = Quaternion.Euler(rotation);
    }
}
