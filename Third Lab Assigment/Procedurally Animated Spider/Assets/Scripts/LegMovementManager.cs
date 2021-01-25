using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMovementManager : MonoBehaviour
{
    public static LegMovementManager Instance { get; private set; }

    [SerializeField] private List<IKLegTarget> _leftTargets;
    [SerializeField] private List<IKLegTarget> _rightTargets;
    [SerializeField] private float _baseHeight;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
        {
            Destroy(gameObject);
            return;
        }
    }

    public bool CanMakeStep(IKLegTarget target) 
    {
        if (_leftTargets.Contains(target))
            return MakeACheck(_leftTargets, _rightTargets, _leftTargets.IndexOf(target));
        else
            return MakeACheck(_rightTargets, _leftTargets, _rightTargets.IndexOf(target));
    }

    private static bool MakeACheck(List<IKLegTarget> targetsSide, List<IKLegTarget> oppositeSide, int index) 
    {
        int indexCheck = (index + 1) % targetsSide.Count;
        return oppositeSide[index].IsGrounded && targetsSide[indexCheck].IsGrounded;
    }

    public Quaternion CalculateLegVectors(out Vector3 widthLegDiff, out Vector3 lengthLegDiff)
    {
        Vector3 leftAvg = Vector3.zero, rightAvg = Vector3.zero, frontAvg = Vector3.zero, backAvg = Vector3.zero;
        for (int i = 0; i < _leftTargets.Count; ++i)
        {
            Vector3 leftPos = _leftTargets[i].Transform.position;
            Vector3 rightPos = _rightTargets[i].Transform.position;
            leftAvg += leftPos;
            rightAvg += rightPos;
            if (i < _leftTargets.Count / 2)
                frontAvg += leftPos + rightPos;
            else
                backAvg += leftPos + rightPos;
        }
        leftAvg /= _leftTargets.Count;
        rightAvg /= _rightTargets.Count;
        frontAvg /= _leftTargets.Count;
        backAvg /= _leftTargets.Count;
        widthLegDiff = rightAvg - leftAvg;
        lengthLegDiff = frontAvg - backAvg;
        return Quaternion.LookRotation(lengthLegDiff, Vector3.Cross(lengthLegDiff, widthLegDiff));
    }

    public float GetSpiderY() 
    {
        float ySum = 0;
        for (int i = 0; i < _leftTargets.Count; ++i) 
            ySum += _leftTargets[i].Transform.position.y + _rightTargets[i].Transform.position.y;
        return ySum / (_leftTargets.Count + _rightTargets.Count) + _baseHeight;
    }
}