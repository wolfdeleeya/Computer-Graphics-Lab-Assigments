using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMovementManager : MonoBehaviour
{
    public static LegMovementManager Instance { get; private set; }

    [SerializeField] private List<IKLegTarget> _leftTargets;
    [SerializeField] private List<IKLegTarget> _rightTargets;
    [SerializeField] private float _rotationMultiplier;
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
        int indexCheck = index % 2 == 0 ? 1 : -1;
        return oppositeSide[index].IsGrounded && targetsSide[index + indexCheck].IsGrounded;
    }

    public Vector3 GetEulerSpiderRotation() 
    {
        float leftAmount = 0, rightAmount = 0, frontAmount = 0, backAmount = 0;
        for (int i = 0; i < _leftTargets.Count; ++i) 
        {
            leftAmount += _leftTargets[i].Transform.position.y;
            rightAmount += _rightTargets[i].Transform.position.y;
            if (i >= _leftTargets.Count / 2)        //BACK LEGS
                backAmount += (_leftTargets[i].Transform.position.y + _rightTargets[i].Transform.position.y);
            else                                    //FRONT LEGS
                frontAmount += (_leftTargets[i].Transform.position.y + _rightTargets[i].Transform.position.y);
        }
        return new Vector3(Mathf.Clamp((rightAmount - leftAmount) * _rotationMultiplier,-90, 90), 0, Mathf.Clamp((frontAmount - backAmount) * _rotationMultiplier, -90, 90));
    }

    public float GetSpiderY() 
    {
        float ySum = 0;
        for (int i = 0; i < _leftTargets.Count; ++i) 
            ySum += _leftTargets[i].Transform.position.y + _rightTargets[i].Transform.position.y;
        return ySum / (_leftTargets.Count + _rightTargets.Count) + _baseHeight;
    }
}