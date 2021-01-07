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
        int indexCheck = (index + 1)%targetsSide.Count;
        return oppositeSide[index].IsGrounded && targetsSide[indexCheck].IsGrounded;
    }

    public float GetBaseHeight() { return _baseHeight; }
}