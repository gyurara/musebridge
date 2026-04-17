using UnityEngine;

/// <summary>
/// 고정된 가시 장애물. 닿으면 즉사 (낙사 처리)
/// </summary>
public class SpikeObstacle : ObstacleBase
{
    private void Awake() => isLethal = true;
}
