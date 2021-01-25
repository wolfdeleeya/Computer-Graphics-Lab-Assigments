using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventBool : UnityEvent<bool> { }

public class IKLegTarget : MonoBehaviour
{
    public Transform Transform { get; private set; }
    public UnityEventBool StateChanged;

    [Header("Step Properties")]
    [SerializeField] private float _stepLength;
    [SerializeField] private float _stepHeight;
    [SerializeField] private float _stepWidth;

    private Vector3 _startPos, _endPos;
    private QuadraticInterpolator _interpolator;
    private Coroutine _c;
    private bool _isGrounded;

    public bool IsGrounded {
        get 
        {
            return _isGrounded;
        }
        private set 
        {
            _isGrounded = value;
            StateChanged.Invoke(_isGrounded);
        }
    }

    private void Awake()
    {
        Transform = transform;
        StateChanged = new UnityEventBool();
        IsGrounded = true;
        _interpolator = new QuadraticInterpolator(_stepHeight, _stepWidth);
    }

    public void Move(Vector3 goal) 
    {
        IsGrounded = false;
        _interpolator.InitializeInterpolation(Transform.position, goal);
        if (_c != null)
            StopCoroutine(_c);
        _c = StartCoroutine("Hop");
    }

    private IEnumerator Hop() 
    {
        float t = 0;
        while (t < _stepLength) 
        {
            t += Time.deltaTime;
            Transform.position = _interpolator.Interpolate(t / _stepLength);
            yield return null;
        }
        IsGrounded = true;
    }

    private class QuadraticInterpolator 
    {
        private float _height, _width;
        private float _beginningX, _endingX, _differenceFromSmaller;
        private Vector3 _beginningPos, _endingPos;

        public QuadraticInterpolator(float height, float width) 
        {
            _height = height;
            _width = -1/width;
        }

        public void InitializeInterpolation(Vector3 startPos, Vector3 endPos)
        {
            _beginningPos = startPos;
            _endingPos = endPos;
            float startY = startPos.y;
            float endY = endPos.y;

            if (startY > endY) 
            {
                _beginningX = -CalculateXForY(startY - endY);
                _endingX = CalculateXForY();
                _differenceFromSmaller = endY;
            }
            else if (startY < endY) 
            {
                _beginningX = -CalculateXForY();
                _endingX = CalculateXForY(endY - startY);
                _differenceFromSmaller = startY;
            }
            else 
            {
                _beginningX = -CalculateXForY();
                _endingX = -_beginningX;
                _differenceFromSmaller = startY;
            }
        }

        public Vector3 Interpolate(float t) 
        {
            float lerpedX = Mathf.Lerp(_beginningX, _endingX, t);
            Vector3 lerpedPos = Vector3.Lerp(_beginningPos, _endingPos, t);
            return new Vector3(lerpedPos.x ,_width * Mathf.Pow(lerpedX, 2) + _height + _differenceFromSmaller, lerpedPos.z);
        }

        private float CalculateXForY(float y = 0) 
        {
            float c = _height - y;
            return Mathf.Sqrt(Mathf.Abs(4 * _width * c)) / (2 * _width);
        }
    }
}